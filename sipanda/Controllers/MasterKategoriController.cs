using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sipanda.Data;
using Sipanda.Models.Entities;

namespace Sipanda.Controllers
{
    [Authorize]
    public class MasterKategoriController : Controller
    {
        private readonly SipandaDbContext _context;

        public MasterKategoriController(SipandaDbContext context)
        {
            _context = context;
        }

        // GET: MasterKategori
        public async Task<IActionResult> Index(int? parentFilter)
        {
            var query = _context.MasterKategori
                .Include(k => k.Parent)
                .Include(k => k.SubKategoriList)
                .Include(k => k.KomponenList)
                .AsQueryable();

            if (parentFilter.HasValue)
            {
                if (parentFilter.Value == 0)
                {
                    // Hanya Kategori Induk
                    query = query.Where(k => k.ParentId == null);
                }
                else
                {
                    // Filter berdasarkan Parent ID tertentu
                    query = query.Where(k => k.ParentId == parentFilter.Value);
                }
            }

            var data = await query
                .OrderBy(k => k.ParentId.HasValue ? k.Parent!.NamaKategori : k.NamaKategori)
                .ThenBy(k => k.NamaKategori)
                .ToListAsync();

            ViewData["ParentList"] = new SelectList(await _context.MasterKategori.Where(k => k.ParentId == null).OrderBy(k => k.NamaKategori).ToListAsync(), "Id", "NamaKategori");
            ViewData["CurrentFilter"] = parentFilter;

            return View(data);
        }

        // GET: MasterKategori/Create
        public async Task<IActionResult> Create(int? parentId)
        {
            var parentCategories = await _context.MasterKategori
                .Where(k => k.ParentId == null)
                .OrderBy(k => k.NamaKategori)
                .ToListAsync();

            ViewData["ParentId"] = new SelectList(parentCategories, "Id", "NamaKategori", parentId);
            return View(new MasterKategori { ParentId = parentId });
        }

        // POST: MasterKategori/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NamaKategori,ParentId")] MasterKategori masterKategori)
        {
            if (ModelState.IsValid)
            {
                _context.Add(masterKategori);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Data Kategori berhasil ditambahkan.";
                return RedirectToAction(nameof(Index));
            }

            var parentCategories = await _context.MasterKategori
                .Where(k => k.ParentId == null)
                .OrderBy(k => k.NamaKategori)
                .ToListAsync();

            ViewData["ParentId"] = new SelectList(parentCategories, "Id", "NamaKategori", masterKategori.ParentId);
            return View(masterKategori);
        }

        // GET: MasterKategori/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var masterKategori = await _context.MasterKategori.FindAsync(id);
            if (masterKategori == null) return NotFound();

            // Hindari memilih dirinya sendiri sebagai parent
            var parentCategories = await _context.MasterKategori
                .Where(k => k.ParentId == null && k.Id != id)
                .OrderBy(k => k.NamaKategori)
                .ToListAsync();

            ViewData["ParentId"] = new SelectList(parentCategories, "Id", "NamaKategori", masterKategori.ParentId);
            return View(masterKategori);
        }

        // POST: MasterKategori/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NamaKategori,ParentId")] MasterKategori masterKategori)
        {
            if (id != masterKategori.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(masterKategori);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Data Kategori berhasil diperbarui.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.MasterKategori.AnyAsync(e => e.Id == masterKategori.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            var parentCategories = await _context.MasterKategori
                .Where(k => k.ParentId == null && k.Id != id)
                .OrderBy(k => k.NamaKategori)
                .ToListAsync();

            ViewData["ParentId"] = new SelectList(parentCategories, "Id", "NamaKategori", masterKategori.ParentId);
            return View(masterKategori);
        }

        // POST: MasterKategori/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var masterKategori = await _context.MasterKategori
                .Include(k => k.SubKategoriList)
                .Include(k => k.KomponenList)
                .Include(k => k.TarifList)
                .FirstOrDefaultAsync(k => k.Id == id);

            if (masterKategori == null) return NotFound();

            if (masterKategori.SubKategoriList.Any() || masterKategori.KomponenList.Any() || masterKategori.TarifList.Any())
            {
                TempData["ErrorMessage"] = "Tidak dapat menghapus Kategori ini karena memiliki sub-kategori, komponen layanan, atau data tarif terkait.";
                return RedirectToAction(nameof(Index));
            }

            _context.MasterKategori.Remove(masterKategori);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Data Kategori berhasil dihapus.";
            return RedirectToAction(nameof(Index));
        }
    }
}
