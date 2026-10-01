using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sipanda.Data;
using Sipanda.Models.Entities;

namespace Sipanda.Controllers
{
    [Authorize]
    public class MasterSkpdController : Controller
    {
        private readonly SipandaDbContext _context;

        public MasterSkpdController(SipandaDbContext context)
        {
            _context = context;
        }

        // GET: MasterSkpd (dengan Hierarki Induk - Anak, Filter, Pencarian, & Pagination)
        public async Task<IActionResult> Index(string? keyword, int? parentFilter, int page = 1)
        {
            var query = _context.MasterSkpd
                .Include(s => s.Parent)
                .Include(s => s.SubSkpdList)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                query = query.Where(s => s.KodeSkpd.ToLower().Contains(kw) || 
                                         s.NamaSkpd.ToLower().Contains(kw) ||
                                         (s.Parent != null && s.Parent.NamaSkpd.ToLower().Contains(kw)));
            }

            if (parentFilter.HasValue)
            {
                if (parentFilter.Value == 0)
                {
                    // Hanya OPD Induk (Level 1)
                    query = query.Where(s => s.ParentId == null);
                }
                else
                {
                    // Filter unit di bawah OPD Induk tertentu
                    query = query.Where(s => s.ParentId == parentFilter.Value || s.Id == parentFilter.Value);
                }
            }

            int pageSize = 25;
            int totalItems = await query.CountAsync();

            var data = await query
                .OrderBy(s => s.ParentId.HasValue ? s.Parent!.NamaSkpd : s.NamaSkpd)
                .ThenBy(s => s.ParentId == null ? 0 : 1)
                .ThenBy(s => s.KodeSkpd)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["Keyword"] = keyword;
            ViewData["ParentFilter"] = parentFilter;
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewData["TotalItems"] = totalItems;
            ViewData["ParentList"] = new SelectList(
                await _context.MasterSkpd.Where(s => s.ParentId == null).OrderBy(s => s.NamaSkpd).ToListAsync(),
                "Id", "NamaSkpd", parentFilter);

            return View(data);
        }

        // GET: MasterSkpd/Create
        public async Task<IActionResult> Create(int? parentId)
        {
            await PopulateParentDropdown(parentId);
            return View(new MasterSkpd { ParentId = parentId });
        }

        // POST: MasterSkpd/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("KodeSkpd,NamaSkpd,ParentId")] MasterSkpd masterSkpd)
        {
            if (ModelState.IsValid)
            {
                _context.Add(masterSkpd);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Data SKPD / Unit Kerja berhasil ditambahkan.";
                return RedirectToAction(nameof(Index));
            }
            await PopulateParentDropdown(masterSkpd.ParentId);
            return View(masterSkpd);
        }

        // GET: MasterSkpd/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var masterSkpd = await _context.MasterSkpd.FindAsync(id);
            if (masterSkpd == null) return NotFound();

            await PopulateParentDropdown(masterSkpd.ParentId, id);
            return View(masterSkpd);
        }

        // POST: MasterSkpd/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,KodeSkpd,NamaSkpd,ParentId")] MasterSkpd masterSkpd)
        {
            if (id != masterSkpd.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(masterSkpd);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Data SKPD / Unit Kerja berhasil diperbarui.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.MasterSkpd.AnyAsync(e => e.Id == masterSkpd.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            await PopulateParentDropdown(masterSkpd.ParentId, id);
            return View(masterSkpd);
        }

        // POST: MasterSkpd/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var masterSkpd = await _context.MasterSkpd
                .Include(s => s.SubSkpdList)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (masterSkpd == null) return NotFound();

            if (masterSkpd.SubSkpdList.Any())
            {
                TempData["ErrorMessage"] = $"Tidak dapat menghapus OPD ini karena masih memiliki {masterSkpd.SubSkpdList.Count} unit kerja / puskesmas di bawahnya.";
                return RedirectToAction(nameof(Index));
            }

            _context.MasterSkpd.Remove(masterSkpd);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Data SKPD / Unit Kerja berhasil dihapus.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateParentDropdown(int? selectedParentId = null, int? excludeId = null)
        {
            var query = _context.MasterSkpd.Where(s => s.ParentId == null);
            if (excludeId.HasValue)
            {
                query = query.Where(s => s.Id != excludeId.Value);
            }

            var parentList = await query.OrderBy(s => s.NamaSkpd).ToListAsync();
            ViewData["ParentId"] = new SelectList(parentList, "Id", "NamaSkpd", selectedParentId);
        }
    }
}
