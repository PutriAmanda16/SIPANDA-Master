using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sipanda.Data;
using Sipanda.Models.Entities;

namespace Sipanda.Controllers
{
    [Authorize]
    public class MasterPeraturanController : Controller
    {
        private readonly SipandaDbContext _context;

        public MasterPeraturanController(SipandaDbContext context)
        {
            _context = context;
        }

        // GET: MasterPeraturan
        public async Task<IActionResult> Index()
        {
            var data = await _context.MasterPeraturan
                .Include(p => p.TarifList)
                .OrderByDescending(p => p.Tahun)
                .ThenByDescending(p => p.TanggalBerlaku)
                .ToListAsync();
            return View(data);
        }

        // GET: MasterPeraturan/Create
        public IActionResult Create()
        {
            return View(new MasterPeraturan { TanggalBerlaku = DateOnly.FromDateTime(DateTime.Today), Tahun = (short)DateTime.Today.Year });
        }

        // POST: MasterPeraturan/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("JenisPeraturan,Nomor,Tahun,Tentang,TanggalBerlaku,Status")] MasterPeraturan masterPeraturan)
        {
            if (ModelState.IsValid)
            {
                _context.Add(masterPeraturan);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Data Regulasi / Peraturan berhasil ditambahkan.";
                return RedirectToAction(nameof(Index));
            }
            return View(masterPeraturan);
        }

        // GET: MasterPeraturan/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var masterPeraturan = await _context.MasterPeraturan.FindAsync(id);
            if (masterPeraturan == null) return NotFound();

            return View(masterPeraturan);
        }

        // POST: MasterPeraturan/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,JenisPeraturan,Nomor,Tahun,Tentang,TanggalBerlaku,Status")] MasterPeraturan masterPeraturan)
        {
            if (id != masterPeraturan.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(masterPeraturan);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Data Regulasi / Peraturan berhasil diperbarui.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.MasterPeraturan.AnyAsync(e => e.Id == masterPeraturan.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(masterPeraturan);
        }

        // POST: MasterPeraturan/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var masterPeraturan = await _context.MasterPeraturan
                .Include(p => p.TarifList)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (masterPeraturan == null) return NotFound();

            if (masterPeraturan.TarifList.Any())
            {
                TempData["ErrorMessage"] = "Tidak dapat menghapus Peraturan ini karena memiliki data tarif retribusi terkait.";
                return RedirectToAction(nameof(Index));
            }

            _context.MasterPeraturan.Remove(masterPeraturan);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Data Regulasi / Peraturan berhasil dihapus.";
            return RedirectToAction(nameof(Index));
        }
    }
}
