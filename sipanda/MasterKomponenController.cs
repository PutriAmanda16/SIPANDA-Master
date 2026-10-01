using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sipanda.Data;
using Sipanda.Models.Entities;

namespace Sipanda.Controllers
{
    [Authorize]
    public class MasterKomponenController : Controller
    {
        private readonly SipandaDbContext _context;

        public MasterKomponenController(SipandaDbContext context)
        {
            _context = context;
        }

        // GET: MasterKomponen (dengan Pencarian, Filter Kategori, dan Pagination)
        public async Task<IActionResult> Index(string? keyword, int? kategoriId, int page = 1)
        {
            var query = _context.MasterKomponen
                .Include(k => k.Kategori)
                .ThenInclude(kat => kat!.Parent)
                .Include(k => k.TarifList)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                query = query.Where(k => k.NamaKomponen.ToLower().Contains(kw) || 
                                         (k.Satuan != null && k.Satuan.ToLower().Contains(kw)) ||
                                         k.Kategori!.NamaKategori.ToLower().Contains(kw));
            }

            if (kategoriId.HasValue && kategoriId.Value > 0)
            {
                query = query.Where(k => k.KategoriId == kategoriId.Value || 
                                         k.Kategori!.ParentId == kategoriId.Value);
            }

            int pageSize = 25;
            int totalItems = await query.CountAsync();

            var items = await query
                .OrderBy(k => k.Kategori!.NamaKategori)
                .ThenBy(k => k.NamaKomponen)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["Keyword"] = keyword;
            ViewData["KategoriId"] = kategoriId;
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewData["TotalItems"] = totalItems;
            ViewData["KategoriList"] = new SelectList(await _context.MasterKategori.OrderBy(k => k.NamaKategori).ToListAsync(), "Id", "NamaKategori", kategoriId);

            return View(items);
        }

        // GET: MasterKomponen/Create
        public async Task<IActionResult> Create(int? kategoriId)
        {
            ViewData["KategoriId"] = new SelectList(await _context.MasterKategori.OrderBy(k => k.NamaKategori).ToListAsync(), "Id", "NamaKategori", kategoriId);
            return View();
        }

        // POST: MasterKomponen/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("KategoriId,NamaKomponen,Satuan")] MasterKomponen masterKomponen)
        {
            if (ModelState.IsValid)
            {
                _context.Add(masterKomponen);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Data Komponen Layanan berhasil ditambahkan.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["KategoriId"] = new SelectList(await _context.MasterKategori.OrderBy(k => k.NamaKategori).ToListAsync(), "Id", "NamaKategori", masterKomponen.KategoriId);
            return View(masterKomponen);
        }

        // GET: MasterKomponen/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var masterKomponen = await _context.MasterKomponen.FindAsync(id);
            if (masterKomponen == null) return NotFound();

            ViewData["KategoriId"] = new SelectList(await _context.MasterKategori.OrderBy(k => k.NamaKategori).ToListAsync(), "Id", "NamaKategori", masterKomponen.KategoriId);
            return View(masterKomponen);
        }

        // POST: MasterKomponen/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,KategoriId,NamaKomponen,Satuan")] MasterKomponen masterKomponen)
        {
            if (id != masterKomponen.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(masterKomponen);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Data Komponen Layanan berhasil diperbarui.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.MasterKomponen.AnyAsync(e => e.Id == masterKomponen.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["KategoriId"] = new SelectList(await _context.MasterKategori.OrderBy(k => k.NamaKategori).ToListAsync(), "Id", "NamaKategori", masterKomponen.KategoriId);
            return View(masterKomponen);
        }

        // POST: MasterKomponen/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var masterKomponen = await _context.MasterKomponen
                .Include(k => k.TarifList)
                .FirstOrDefaultAsync(k => k.Id == id);

            if (masterKomponen == null) return NotFound();

            if (masterKomponen.TarifList.Any())
            {
                TempData["ErrorMessage"] = "Tidak dapat menghapus Komponen ini karena masih memiliki data tarif terkait.";
                return RedirectToAction(nameof(Index));
            }

            _context.MasterKomponen.Remove(masterKomponen);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Data Komponen Layanan berhasil dihapus.";
            return RedirectToAction(nameof(Index));
        }
    }
}
