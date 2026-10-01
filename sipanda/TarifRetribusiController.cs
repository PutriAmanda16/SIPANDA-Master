using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sipanda.Data;
using Sipanda.Models.Entities;
using Sipanda.Models.ViewModels;

namespace Sipanda.Controllers
{
    [Authorize]
    public class TarifRetribusiController : Controller
    {
        private readonly SipandaDbContext _context;

        public TarifRetribusiController(SipandaDbContext context)
        {
            _context = context;
        }

        // GET: TarifRetribusi
        public async Task<IActionResult> Index(
            string? keyword,
            int? faskesId,
            int? kategoriId,
            int? peraturanId,
            string? status,
            int page = 1)
        {
            var query = _context.VTarifLengkap.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                query = query.Where(t =>
                    (t.NamaKomponen != null && t.NamaKomponen.ToLower().Contains(kw)) ||
                    (t.NamaKategori != null && t.NamaKategori.ToLower().Contains(kw)) ||
                    (t.KelompokKategori != null && t.KelompokKategori.ToLower().Contains(kw)) ||
                    (t.NamaFaskes != null && t.NamaFaskes.ToLower().Contains(kw)) ||
                    (t.KodeRekening != null && t.KodeRekening.ToLower().Contains(kw)));
            }

            if (faskesId.HasValue && faskesId.Value > 0)
            {
                query = query.Where(t => t.FaskesId == faskesId.Value);
            }

            if (kategoriId.HasValue && kategoriId.Value > 0)
            {
                var childKategoriIds = await _context.MasterKategori
                    .Where(k => k.ParentId == kategoriId.Value)
                    .Select(k => k.Id)
                    .ToListAsync();

                childKategoriIds.Add(kategoriId.Value);
                query = query.Where(t => childKategoriIds.Contains(t.KategoriId));
            }

            if (peraturanId.HasValue && peraturanId.Value > 0)
            {
                query = query.Where(t => t.PeraturanId == peraturanId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(t => t.Status == status);
            }

            int pageSize = 20;
            int totalItems = await query.CountAsync();

            var items = await query
                .OrderBy(t => t.KodeFaskes)
                .ThenBy(t => t.KelompokKategori)
                .ThenBy(t => t.NamaKategori)
                .ThenBy(t => t.NamaKomponen)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var vm = new TarifFilterViewModel
            {
                Keyword = keyword,
                FaskesId = faskesId,
                KategoriId = kategoriId,
                PeraturanId = peraturanId,
                Status = status,
                PageNumber = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                Items = items,
                AvailableFaskes = await _context.MasterFaskes.OrderBy(f => f.KodeFaskes).ToListAsync(),
                AvailableKategori = await _context.MasterKategori.OrderBy(k => k.NamaKategori).ToListAsync(),
                AvailablePeraturan = await _context.MasterPeraturan.OrderByDescending(p => p.Tahun).ToListAsync()
            };

            return View(vm);
        }

        // GET: TarifRetribusi/Create
        public async Task<IActionResult> Create(int? faskesId, int? kategoriId)
        {
            await PopulateDropdowns(faskesId, kategoriId);
            return View(new TarifRetribusi
            {
                TanggalMulai = DateOnly.FromDateTime(DateTime.Today),
                Status = "aktif"
            });
        }

        // POST: TarifRetribusi/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PeraturanId,FaskesId,KategoriId,KomponenId,Harga,TanggalMulai,TanggalAkhir,Status")] TarifRetribusi tarifRetribusi)
        {
            // Validasi KategoriId harus selaras dengan KomponenId jika perlu
            var komponen = await _context.MasterKomponen.FindAsync(tarifRetribusi.KomponenId);
            if (komponen != null)
            {
                tarifRetribusi.KategoriId = komponen.KategoriId;
            }

            if (ModelState.IsValid)
            {
                _context.Add(tarifRetribusi);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Data Tarif Retribusi berhasil ditambahkan.";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdowns(tarifRetribusi.FaskesId, tarifRetribusi.KategoriId, tarifRetribusi.PeraturanId, tarifRetribusi.KomponenId);
            return View(tarifRetribusi);
        }

        // GET: TarifRetribusi/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var tarifRetribusi = await _context.TarifRetribusi
                .Include(t => t.Komponen)
                .Include(t => t.Kategori)
                .Include(t => t.Faskes)
                .Include(t => t.Peraturan)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tarifRetribusi == null) return NotFound();

            await PopulateDropdowns(tarifRetribusi.FaskesId, tarifRetribusi.KategoriId, tarifRetribusi.PeraturanId, tarifRetribusi.KomponenId);
            return View(tarifRetribusi);
        }

        // POST: TarifRetribusi/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PeraturanId,FaskesId,KategoriId,KomponenId,Harga,TanggalMulai,TanggalAkhir,Status")] TarifRetribusi tarifRetribusi)
        {
            if (id != tarifRetribusi.Id) return NotFound();

            var komponen = await _context.MasterKomponen.FindAsync(tarifRetribusi.KomponenId);
            if (komponen != null)
            {
                tarifRetribusi.KategoriId = komponen.KategoriId;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tarifRetribusi);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Data Tarif Retribusi berhasil diperbarui.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.TarifRetribusi.AnyAsync(e => e.Id == tarifRetribusi.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdowns(tarifRetribusi.FaskesId, tarifRetribusi.KategoriId, tarifRetribusi.PeraturanId, tarifRetribusi.KomponenId);
            return View(tarifRetribusi);
        }

        // POST: TarifRetribusi/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var tarifRetribusi = await _context.TarifRetribusi.FindAsync(id);
            if (tarifRetribusi == null) return NotFound();

            _context.TarifRetribusi.Remove(tarifRetribusi);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Data Tarif Retribusi berhasil dihapus.";
            return RedirectToAction(nameof(Index));
        }

        // AJAX endpoint untuk mendapatkan komponen berdasarkan Kategori
        [HttpGet]
        public async Task<IActionResult> GetKomponenByKategori(int kategoriId)
        {
            var list = await _context.MasterKomponen
                .Where(k => k.KategoriId == kategoriId)
                .OrderBy(k => k.NamaKomponen)
                .Select(k => new { k.Id, k.NamaKomponen, k.Satuan })
                .ToListAsync();

            return Json(list);
        }

        private async Task PopulateDropdowns(int? faskesId = null, int? kategoriId = null, int? peraturanId = null, int? komponenId = null)
        {
            ViewData["PeraturanId"] = new SelectList(
                await _context.MasterPeraturan.OrderByDescending(p => p.Tahun).ToListAsync(),
                "Id", "JudulLengkap", peraturanId);

            ViewData["FaskesId"] = new SelectList(
                await _context.MasterFaskes.OrderBy(f => f.KodeFaskes).ToListAsync(),
                "Id", "NamaFaskes", faskesId);

            ViewData["KategoriId"] = new SelectList(
                await _context.MasterKategori.OrderBy(k => k.NamaKategori).ToListAsync(),
                "Id", "NamaKategori", kategoriId);

            var komponenQuery = _context.MasterKomponen.AsQueryable();
            if (kategoriId.HasValue && kategoriId.Value > 0)
            {
                komponenQuery = komponenQuery.Where(k => k.KategoriId == kategoriId.Value);
            }

            ViewData["KomponenId"] = new SelectList(
                await komponenQuery.OrderBy(k => k.NamaKomponen).Take(200).ToListAsync(),
                "Id", "NamaKomponen", komponenId);
        }
    }
}
