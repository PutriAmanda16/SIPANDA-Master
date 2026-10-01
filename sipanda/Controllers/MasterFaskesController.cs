using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sipanda.Data;
using Sipanda.Models.Entities;

namespace Sipanda.Controllers
{
    [Authorize]
    public class MasterFaskesController : Controller
    {
        private readonly SipandaDbContext _context;

        public MasterFaskesController(SipandaDbContext context)
        {
            _context = context;
        }

        // GET: MasterFaskes
        public async Task<IActionResult> Index()
        {
            var data = await _context.MasterFaskes
                .Include(f => f.Rekening)
                .Include(f => f.TarifList)
                .OrderBy(f => f.KodeFaskes)
                .ToListAsync();
            return View(data);
        }

        // GET: MasterFaskes/Create
        public async Task<IActionResult> Create()
        {
            ViewData["RekeningId"] = new SelectList(await _context.MasterRekening.OrderBy(r => r.KodeRekening).ToListAsync(), "Id", "NamaRekening");
            return View();
        }

        // POST: MasterFaskes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("KodeFaskes,NamaFaskes,RekeningId")] MasterFaskes masterFaskes)
        {
            if (ModelState.IsValid)
            {
                if (await _context.MasterFaskes.AnyAsync(f => f.KodeFaskes == masterFaskes.KodeFaskes))
                {
                    ModelState.AddModelError("KodeFaskes", "Kode Faskes sudah terdaftar.");
                    ViewData["RekeningId"] = new SelectList(await _context.MasterRekening.OrderBy(r => r.KodeRekening).ToListAsync(), "Id", "NamaRekening", masterFaskes.RekeningId);
                    return View(masterFaskes);
                }

                _context.Add(masterFaskes);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Data Fasilitas Kesehatan berhasil ditambahkan.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["RekeningId"] = new SelectList(await _context.MasterRekening.OrderBy(r => r.KodeRekening).ToListAsync(), "Id", "NamaRekening", masterFaskes.RekeningId);
            return View(masterFaskes);
        }

        // GET: MasterFaskes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var masterFaskes = await _context.MasterFaskes.FindAsync(id);
            if (masterFaskes == null) return NotFound();

            ViewData["RekeningId"] = new SelectList(await _context.MasterRekening.OrderBy(r => r.KodeRekening).ToListAsync(), "Id", "NamaRekening", masterFaskes.RekeningId);
            return View(masterFaskes);
        }

        // POST: MasterFaskes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,KodeFaskes,NamaFaskes,RekeningId")] MasterFaskes masterFaskes)
        {
            if (id != masterFaskes.Id) return NotFound();

            if (ModelState.IsValid)
            {
                if (await _context.MasterFaskes.AnyAsync(f => f.KodeFaskes == masterFaskes.KodeFaskes && f.Id != id))
                {
                    ModelState.AddModelError("KodeFaskes", "Kode Faskes sudah digunakan oleh data lain.");
                    ViewData["RekeningId"] = new SelectList(await _context.MasterRekening.OrderBy(r => r.KodeRekening).ToListAsync(), "Id", "NamaRekening", masterFaskes.RekeningId);
                    return View(masterFaskes);
                }

                try
                {
                    _context.Update(masterFaskes);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Data Fasilitas Kesehatan berhasil diperbarui.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.MasterFaskes.AnyAsync(e => e.Id == masterFaskes.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["RekeningId"] = new SelectList(await _context.MasterRekening.OrderBy(r => r.KodeRekening).ToListAsync(), "Id", "NamaRekening", masterFaskes.RekeningId);
            return View(masterFaskes);
        }

        // POST: MasterFaskes/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var masterFaskes = await _context.MasterFaskes
                .Include(f => f.TarifList)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (masterFaskes == null) return NotFound();

            if (masterFaskes.TarifList.Any())
            {
                TempData["ErrorMessage"] = "Tidak dapat menghapus Faskes ini karena memiliki data tarif retribusi terkait.";
                return RedirectToAction(nameof(Index));
            }

            _context.MasterFaskes.Remove(masterFaskes);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Data Fasilitas Kesehatan berhasil dihapus.";
            return RedirectToAction(nameof(Index));
        }
    }
}
