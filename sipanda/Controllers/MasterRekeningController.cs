using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sipanda.Data;
using Sipanda.Models.Entities;

namespace Sipanda.Controllers
{
    [Authorize]
    public class MasterRekeningController : Controller
    {
        private readonly SipandaDbContext _context;

        public MasterRekeningController(SipandaDbContext context)
        {
            _context = context;
        }

        // GET: MasterRekening
        public async Task<IActionResult> Index()
        {
            var data = await _context.MasterRekening
                .Include(r => r.FaskesList)
                .OrderBy(r => r.KodeRekening)
                .ToListAsync();
            return View(data);
        }

        // GET: MasterRekening/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MasterRekening/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("KodeRekening,NamaRekening")] MasterRekening masterRekening)
        {
            if (ModelState.IsValid)
            {
                // Cek duplikasi
                if (await _context.MasterRekening.AnyAsync(r => r.KodeRekening == masterRekening.KodeRekening))
                {
                    ModelState.AddModelError("KodeRekening", "Kode Rekening sudah terdaftar.");
                    return View(masterRekening);
                }

                _context.Add(masterRekening);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Data Rekening berhasil ditambahkan.";
                return RedirectToAction(nameof(Index));
            }
            return View(masterRekening);
        }

        // GET: MasterRekening/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var masterRekening = await _context.MasterRekening.FindAsync(id);
            if (masterRekening == null) return NotFound();

            return View(masterRekening);
        }

        // POST: MasterRekening/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,KodeRekening,NamaRekening")] MasterRekening masterRekening)
        {
            if (id != masterRekening.Id) return NotFound();

            if (ModelState.IsValid)
            {
                if (await _context.MasterRekening.AnyAsync(r => r.KodeRekening == masterRekening.KodeRekening && r.Id != id))
                {
                    ModelState.AddModelError("KodeRekening", "Kode Rekening sudah digunakan oleh data lain.");
                    return View(masterRekening);
                }

                try
                {
                    _context.Update(masterRekening);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Data Rekening berhasil diperbarui.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.MasterRekening.AnyAsync(e => e.Id == masterRekening.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(masterRekening);
        }

        // POST: MasterRekening/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var masterRekening = await _context.MasterRekening
                .Include(r => r.FaskesList)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (masterRekening == null) return NotFound();

            if (masterRekening.FaskesList.Any())
            {
                TempData["ErrorMessage"] = "Tidak dapat menghapus Rekening ini karena masih digunakan oleh data Faskes.";
                return RedirectToAction(nameof(Index));
            }

            _context.MasterRekening.Remove(masterRekening);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Data Rekening berhasil dihapus.";
            return RedirectToAction(nameof(Index));
        }
    }
}
