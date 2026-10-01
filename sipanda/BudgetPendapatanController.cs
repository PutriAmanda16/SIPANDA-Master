using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sipanda.Data;
using Sipanda.Models.Entities;
using System.Security.Claims;

namespace Sipanda.Controllers
{
    public class BudgetPendapatanController : Controller
    {
        private readonly SipandaDbContext _context;

        public BudgetPendapatanController(SipandaDbContext context)
        {
            _context = context;
        }

        // GET: BudgetPendapatan (Daftar Dokumen Usulan Anggaran)
        public async Task<IActionResult> Index(int? tahunId, int? skpdId, string? jenisAnggaran, string? status, int page = 1)
        {
            var query = _context.BudgetPendapatan
                .Include(b => b.TahunAnggaran)
                .Include(b => b.Skpd)
                .Include(b => b.CurrentTahap)
                .Include(b => b.VersiList)
                    .ThenInclude(v => v.VersiDetailList)
                .AsQueryable();

            if (tahunId.HasValue) query = query.Where(b => b.TahunAnggaranId == tahunId.Value);
            if (skpdId.HasValue) query = query.Where(b => b.SkpdId == skpdId.Value);
            if (!string.IsNullOrWhiteSpace(jenisAnggaran)) query = query.Where(b => b.JenisAnggaran == jenisAnggaran);
            if (!string.IsNullOrWhiteSpace(status)) query = query.Where(b => b.StatusDokumen == status);

            int pageSize = 15;
            int totalItems = await query.CountAsync();

            var data = await query
                .OrderByDescending(b => b.TahunAnggaran!.Tahun)
                .ThenBy(b => b.Skpd!.NamaSkpd)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.TahunId = new SelectList(await _context.MasterTahunAnggaran.OrderByDescending(t => t.Tahun).ToListAsync(), "Id", "Tahun", tahunId);
            ViewBag.SkpdId = new SelectList(await _context.MasterSkpd.OrderBy(s => s.NamaSkpd).ToListAsync(), "Id", "NamaSkpd", skpdId);
            ViewBag.SelectedJenis = jenisAnggaran;
            ViewBag.SelectedStatus = status;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.TotalItems = totalItems;

            return View(data);
        }

        // GET: BudgetPendapatan/Create
        public async Task<IActionResult> Create()
        {
            await PopulateCreateDropdowns();
            return View(new BudgetPendapatan
            {
                TanggalUsulan = DateTime.Today,
                JenisAnggaran = "MURNI"
            });
        }

        // POST: BudgetPendapatan/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BudgetPendapatan model)
        {
            if (ModelState.IsValid)
            {
                // Auto generate nomor usulan if empty
                var tahunObj = await _context.MasterTahunAnggaran.FindAsync(model.TahunAnggaranId);
                var skpdObj = await _context.MasterSkpd.FindAsync(model.SkpdId);
                var prefix = model.JenisAnggaran == "PERUBAHAN" ? "PAK" : (model.JenisAnggaran == "PERGESERAN" ? "REV" : "MRN");
                var cleanKode = skpdObj?.KodeSkpd.Replace(" ", "") ?? "000";

                if (string.IsNullOrWhiteSpace(model.NomorUsulan))
                {
                    model.NomorUsulan = $"{prefix}/{tahunObj?.Tahun ?? DateTime.Now.Year}/{cleanKode}/{DateTime.Now:MMddHHmm}";
                }

                // Determine initial stage
                int initialTahapId = 1; // Default MRN_USULAN
                if (model.JenisAnggaran == "PERUBAHAN")
                {
                    var tahapPak = await _context.MasterTahapBudgeting.FirstOrDefaultAsync(t => t.KodeTahap == "PAK_EVAL_SMT1");
                    initialTahapId = tahapPak?.Id ?? 14;
                }
                else if (model.JenisAnggaran == "PERGESERAN")
                {
                    var tahapRev = await _context.MasterTahapBudgeting.FirstOrDefaultAsync(t => t.KodeTahap == "REV_USULAN");
                    initialTahapId = tahapRev?.Id ?? 11;
                }

                model.CurrentTahapId = initialTahapId;
                model.StatusDokumen = "DRAFT";
                model.CreatedAt = DateTime.Now;
                model.UpdatedAt = DateTime.Now;

                _context.BudgetPendapatan.Add(model);
                await _context.SaveChangesAsync();

                // Create initial working version V1.0
                var initialVersion = new BudgetPendapatanVersi
                {
                    BudgetId = model.Id,
                    NomorVersi = "V1.0",
                    TahapId = initialTahapId,
                    IsBaseline = false,
                    StatusApproval = "DRAFT",
                    TanggalVersi = DateTime.Now,
                    CatatanPembahasan = $"Penyusunan awal target pendapatan {model.JenisAnggaran}"
                };
                _context.BudgetPendapatanVersi.Add(initialVersion);
                await _context.SaveChangesAsync();

                // If PERUBAHAN & DPA Induk is selected, copy data as Semula
                if (model.JenisAnggaran == "PERUBAHAN" && model.DpaIndukId.HasValue)
                {
                    var dpaIndukVersi = await _context.BudgetPendapatanVersi
                        .Include(v => v.VersiDetailList)
                            .ThenInclude(vd => vd.Detail)
                        .Where(v => v.BudgetId == model.DpaIndukId.Value)
                        .OrderByDescending(v => v.Id)
                        .FirstOrDefaultAsync();

                    if (dpaIndukVersi != null)
                    {
                        foreach (var indukItem in dpaIndukVersi.VersiDetailList)
                        {
                            var newDetail = new BudgetPendapatanDetail
                            {
                                BudgetId = model.Id,
                                RekeningId = indukItem.Detail!.RekeningId,
                                Uraian = indukItem.Detail.Uraian
                            };
                            _context.BudgetPendapatanDetail.Add(newDetail);
                            await _context.SaveChangesAsync();

                            var newVersiDetail = new BudgetPendapatanVersiDetail
                            {
                                VersiId = initialVersion.Id,
                                DetailId = newDetail.Id,
                                TarifRetribusiId = indukItem.TarifRetribusiId,
                                UraianLayanan = indukItem.UraianLayanan,
                                Volume = indukItem.Volume,
                                Satuan = indukItem.Satuan,
                                TarifSatuan = indukItem.TarifSatuan,
                                NilaiTarget = indukItem.NilaiTarget,
                                VolumeSemula = indukItem.Volume,
                                TarifSemula = indukItem.TarifSatuan,
                                NilaiSemula = indukItem.NilaiTarget,
                                Keterangan = "Disalin dari DPA Murni"
                            };
                            _context.BudgetPendapatanVersiDetail.Add(newVersiDetail);
                        }
                        await _context.SaveChangesAsync();
                    }
                }

                TempData["SuccessMessage"] = $"Dokumen usulan anggaran {model.NomorUsulan} berhasil dibuat.";
                return RedirectToAction(nameof(Details), new { id = model.Id });
            }

            await PopulateCreateDropdowns();
            return View(model);
        }

        // GET: BudgetPendapatan/Details/5 (The Budgeting Canvas & Rincian Penghitungan)
        public async Task<IActionResult> Details(int id)
        {
            var document = await _context.BudgetPendapatan
                .Include(b => b.TahunAnggaran)
                .Include(b => b.Skpd)
                .Include(b => b.CurrentTahap)
                .Include(b => b.DpaInduk)
                .Include(b => b.VersiList)
                    .ThenInclude(v => v.Tahap)
                .Include(b => b.VersiList)
                    .ThenInclude(v => v.VersiDetailList)
                        .ThenInclude(vd => vd.Detail)
                            .ThenInclude(d => d!.Rekening)
                .Include(b => b.VersiList)
                    .ThenInclude(v => v.VersiDetailList)
                        .ThenInclude(vd => vd.TarifRetribusi)
                .Include(b => b.VersiList)
                    .ThenInclude(v => v.ApprovalList)
                        .ThenInclude(a => a.ApproverUser)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (document == null) return NotFound();

            // Get active working version (latest)
            var activeVersi = document.VersiList.OrderByDescending(v => v.Id).FirstOrDefault();
            ViewBag.ActiveVersi = activeVersi;

            // Get all stages for workflow stepper
            ViewBag.AllStages = await _context.MasterTahapBudgeting
                .Where(t => t.JenisAnggaran == document.JenisAnggaran && t.IsAktif)
                .OrderBy(t => t.Urutan)
                .ToListAsync();

            // Rekening list for Add Item Modal
            ViewBag.RekeningList = new SelectList(
                await _context.MasterRekening.OrderBy(r => r.KodeRekening).ToListAsync(),
                "Id", "NamaRekening");

            return View(document);
        }

        // POST: BudgetPendapatan/AddItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(int budgetId, int rekeningId, int? tarifRetribusiId, string uraianLayanan, decimal volume, string satuan, long tarifSatuan, string? keterangan, decimal volumeSemula = 0, long tarifSemula = 0)
        {
            var budget = await _context.BudgetPendapatan
                .Include(b => b.VersiList)
                .FirstOrDefaultAsync(b => b.Id == budgetId);

            if (budget == null) return NotFound();

            var activeVersi = budget.VersiList.OrderByDescending(v => v.Id).FirstOrDefault();
            if (activeVersi == null) return BadRequest();

            // 1. Create detail header
            var detail = new BudgetPendapatanDetail
            {
                BudgetId = budgetId,
                RekeningId = rekeningId,
                Uraian = uraianLayanan
            };
            _context.BudgetPendapatanDetail.Add(detail);
            await _context.SaveChangesAsync();

            // 2. Create version detail
            long nilaiTarget = (long)(volume * tarifSatuan);
            long nilaiSemula = (long)(volumeSemula * tarifSemula);

            var versiDetail = new BudgetPendapatanVersiDetail
            {
                VersiId = activeVersi.Id,
                DetailId = detail.Id,
                TarifRetribusiId = tarifRetribusiId,
                UraianLayanan = uraianLayanan,
                Volume = volume,
                Satuan = string.IsNullOrWhiteSpace(satuan) ? "Layanan" : satuan,
                TarifSatuan = tarifSatuan,
                NilaiTarget = nilaiTarget,
                VolumeSemula = volumeSemula,
                TarifSemula = tarifSemula,
                NilaiSemula = nilaiSemula,
                Keterangan = keterangan
            };

            _context.BudgetPendapatanVersiDetail.Add(versiDetail);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Item rincian penghitungan berhasil ditambahkan.";
            return RedirectToAction(nameof(Details), new { id = budgetId });
        }

        // POST: BudgetPendapatan/UpdateItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateItem(int versiDetailId, decimal volume, long tarifSatuan, string? keterangan)
        {
            var item = await _context.BudgetPendapatanVersiDetail
                .Include(vd => vd.Versi)
                .FirstOrDefaultAsync(vd => vd.Id == versiDetailId);

            if (item == null) return NotFound();

            item.Volume = volume;
            item.TarifSatuan = tarifSatuan;
            item.NilaiTarget = (long)(volume * tarifSatuan);
            item.Keterangan = keterangan;

            _context.Update(item);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Item rincian penghitungan berhasil diperbarui.";
            return RedirectToAction(nameof(Details), new { id = item.Versi!.BudgetId });
        }

        // POST: BudgetPendapatan/DeleteItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteItem(int versiDetailId)
        {
            var item = await _context.BudgetPendapatanVersiDetail
                .Include(vd => vd.Versi)
                .Include(vd => vd.Detail)
                .FirstOrDefaultAsync(vd => vd.Id == versiDetailId);

            if (item == null) return NotFound();

            int budgetId = item.Versi!.BudgetId;
            var detailHeader = item.Detail;

            _context.BudgetPendapatanVersiDetail.Remove(item);
            if (detailHeader != null)
            {
                _context.BudgetPendapatanDetail.Remove(detailHeader);
            }
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Item rincian berhasil dihapus.";
            return RedirectToAction(nameof(Details), new { id = budgetId });
        }

        // POST: BudgetPendapatan/SubmitApproval
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitApproval(int budgetId)
        {
            var budget = await _context.BudgetPendapatan
                .Include(b => b.VersiList)
                .FirstOrDefaultAsync(b => b.Id == budgetId);

            if (budget == null) return NotFound();

            var activeVersi = budget.VersiList.OrderByDescending(v => v.Id).FirstOrDefault();
            if (activeVersi == null) return BadRequest();

            budget.StatusDokumen = "DIAJUKAN";
            activeVersi.StatusApproval = "DIAJUKAN";

            _context.Update(budget);
            _context.Update(activeVersi);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Dokumen berhasil diajukan untuk verifikasi / persetujuan tahapan.";
            return RedirectToAction(nameof(Details), new { id = budgetId });
        }

        // POST: BudgetPendapatan/ProcessApproval (Approve / Return / Reject)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessApproval(int budgetId, string actionType, string? catatan)
        {
            var budget = await _context.BudgetPendapatan
                .Include(b => b.CurrentTahap)
                .Include(b => b.VersiList)
                    .ThenInclude(v => v.VersiDetailList)
                .FirstOrDefaultAsync(b => b.Id == budgetId);

            if (budget == null) return NotFound();

            var activeVersi = budget.VersiList.OrderByDescending(v => v.Id).FirstOrDefault();
            if (activeVersi == null) return BadRequest();

            // Get current user id
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.TryParse(userIdStr, out int uid) ? uid : 1;

            if (actionType == "APPROVE")
            {
                // Record approval
                var approval = new BudgetApproval
                {
                    VersiId = activeVersi.Id,
                    TahapId = budget.CurrentTahapId,
                    ApproverUserId = userId,
                    Status = "DISETUJUI",
                    TanggalApproval = DateTime.Now,
                    Catatan = catatan ?? "Disetujui untuk lanjut ke tahapan berikutnya"
                };
                _context.BudgetApproval.Add(approval);

                activeVersi.IsBaseline = true;
                activeVersi.StatusApproval = "DISETUJUI";

                // Check next tahap
                var allStages = await _context.MasterTahapBudgeting
                    .Where(t => t.JenisAnggaran == budget.JenisAnggaran && t.IsAktif)
                    .OrderBy(t => t.Urutan)
                    .ToListAsync();

                var currentIdx = allStages.FindIndex(t => t.Id == budget.CurrentTahapId);
                if (currentIdx >= 0 && currentIdx < allStages.Count - 1)
                {
                    var nextTahap = allStages[currentIdx + 1];
                    budget.CurrentTahapId = nextTahap.Id;
                    budget.StatusDokumen = "DRAFT";

                    // Create new version snapshot for next stage
                    int nextVerNum = int.TryParse(activeVersi.NomorVersi.Replace("V", "").Split('.')[0], out int vn) ? vn + 1 : 2;
                    var nextVersi = new BudgetPendapatanVersi
                    {
                        BudgetId = budget.Id,
                        NomorVersi = $"V{nextVerNum}.0",
                        TahapId = nextTahap.Id,
                        VersiSebelumnyaId = activeVersi.Id,
                        IsBaseline = false,
                        StatusApproval = "DRAFT",
                        TanggalVersi = DateTime.Now,
                        CatatanPembahasan = $"Peralihan ke tahap: {nextTahap.NamaTahap}"
                    };
                    _context.BudgetPendapatanVersi.Add(nextVersi);
                    await _context.SaveChangesAsync();

                    // Duplicate details to new version
                    foreach (var oldItem in activeVersi.VersiDetailList)
                    {
                        var newItem = new BudgetPendapatanVersiDetail
                        {
                            VersiId = nextVersi.Id,
                            DetailId = oldItem.DetailId,
                            TarifRetribusiId = oldItem.TarifRetribusiId,
                            UraianLayanan = oldItem.UraianLayanan,
                            Volume = oldItem.Volume,
                            Satuan = oldItem.Satuan,
                            TarifSatuan = oldItem.TarifSatuan,
                            NilaiTarget = oldItem.NilaiTarget,
                            VolumeSemula = oldItem.VolumeSemula,
                            TarifSemula = oldItem.TarifSemula,
                            NilaiSemula = oldItem.NilaiSemula,
                            Keterangan = oldItem.Keterangan
                        };
                        _context.BudgetPendapatanVersiDetail.Add(newItem);
                    }
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Dokumen disetujui dan berhasil melangkah ke tahap [{nextTahap.NamaTahap}].";
                }
                else
                {
                    // Final stage reached
                    budget.StatusDokumen = "FINAL";
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Dokumen telah mencapai tahapan akhir dan ditetapkan menjadi DPA Final.";
                }
            }
            else if (actionType == "RETURN")
            {
                budget.StatusDokumen = "DIKEMBALIKAN";
                activeVersi.StatusApproval = "DIKEMBALIKAN";

                var approval = new BudgetApproval
                {
                    VersiId = activeVersi.Id,
                    TahapId = budget.CurrentTahapId,
                    ApproverUserId = userId,
                    Status = "DIKEMBALIKAN",
                    TanggalApproval = DateTime.Now,
                    Catatan = catatan ?? "Dikembalikan untuk penyesuaian target"
                };
                _context.BudgetApproval.Add(approval);
                await _context.SaveChangesAsync();

                TempData["ErrorMessage"] = "Dokumen dikembalikan ke OPD untuk dilakukan perbaikan / penyesuaian.";
            }

            return RedirectToAction(nameof(Details), new { id = budgetId });
        }

        // GET: BudgetPendapatan/PrintRkaDpa/5 (Format Resmi Standar SIPD / Permendagri)
        [AllowAnonymous]
        public async Task<IActionResult> PrintRkaDpa(int id)
        {
            var document = await _context.BudgetPendapatan
                .Include(b => b.TahunAnggaran)
                .Include(b => b.Skpd)
                .Include(b => b.CurrentTahap)
                .Include(b => b.VersiList)
                    .ThenInclude(v => v.VersiDetailList)
                        .ThenInclude(vd => vd.Detail)
                            .ThenInclude(d => d!.Rekening)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (document == null) return NotFound();

            var activeVersi = document.VersiList.OrderByDescending(v => v.Id).FirstOrDefault();
            ViewBag.ActiveVersi = activeVersi;

            return View(document);
        }

        // AJAX GET: BudgetPendapatan/SearchTarif?keyword=...
        [HttpGet]
        public async Task<IActionResult> SearchTarif(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return Json(new List<object>());

            var kw = keyword.Trim().ToLower();
            var results = await _context.VTarifLengkap
                .Where(t => (t.NamaKomponen != null && t.NamaKomponen.ToLower().Contains(kw)) ||
                            (t.NamaKategori != null && t.NamaKategori.ToLower().Contains(kw)) ||
                            (t.NamaFaskes != null && t.NamaFaskes.ToLower().Contains(kw)))
                .Take(25)
                .Select(t => new
                {
                    id = t.TarifId,
                    faskes = t.NamaFaskes,
                    kategori = t.NamaKategori,
                    komponen = t.NamaKomponen,
                    satuan = t.Satuan,
                    harga = t.Harga,
                    kodeRekening = t.KodeRekening,
                    namaRekening = t.NamaRekening
                })
                .ToListAsync();

            return Json(results);
        }

        private async Task PopulateCreateDropdowns()
        {
            ViewBag.TahunAnggaranId = new SelectList(
                await _context.MasterTahunAnggaran.Where(t => t.Status != "tutup").OrderByDescending(t => t.Tahun).ToListAsync(),
                "Id", "Tahun");

            ViewBag.SkpdId = new SelectList(
                await _context.MasterSkpd.OrderBy(s => s.NamaSkpd).ToListAsync(),
                "Id", "NamaSkpd");

            ViewBag.DpaIndukList = new SelectList(
                await _context.BudgetPendapatan
                    .Where(b => b.JenisAnggaran == "MURNI")
                    .Include(b => b.Skpd)
                    .OrderByDescending(b => b.CreatedAt)
                    .Select(b => new { b.Id, Label = $"{b.NomorUsulan} - {b.Skpd!.NamaSkpd}" })
                    .ToListAsync(),
                "Id", "Label");
        }
    }
}
