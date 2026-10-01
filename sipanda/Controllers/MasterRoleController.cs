using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sipanda.Data;
using Sipanda.Helpers;
using Sipanda.Models.Entities;

namespace Sipanda.Controllers
{
    [Authorize]
    public class MasterRoleController : Controller
    {
        private readonly SipandaDbContext _context;

        public MasterRoleController(SipandaDbContext context)
        {
            _context = context;
        }

        // GET: MasterRole
        public async Task<IActionResult> Index()
        {
            if (!User.HasMenuAccess(AppMenus.UserManagement))
            {
                return Forbid();
            }

            var roles = await _context.MasterRoles
                .Include(r => r.UserList)
                .OrderBy(r => r.Id)
                .ToListAsync();

            return View(roles);
        }

        // GET: MasterRole/Create
        public IActionResult Create()
        {
            if (!User.HasMenuAccess(AppMenus.UserManagement))
            {
                return Forbid();
            }

            ViewData["AllMenus"] = AppMenus.AllMenus;
            return View(new MasterRole { AllowedMenus = "dashboard" });
        }

        // POST: MasterRole/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NamaRole,Deskripsi")] MasterRole role, string[] selectedMenus)
        {
            if (!User.HasMenuAccess(AppMenus.UserManagement))
            {
                return Forbid();
            }

            role.AllowedMenus = selectedMenus != null && selectedMenus.Length > 0 
                ? string.Join(",", selectedMenus) 
                : "dashboard";

            if (ModelState.IsValid)
            {
                if (await _context.MasterRoles.AnyAsync(r => r.NamaRole == role.NamaRole))
                {
                    ModelState.AddModelError("NamaRole", "Nama Role sudah digunakan.");
                    ViewData["AllMenus"] = AppMenus.AllMenus;
                    return View(role);
                }

                _context.Add(role);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Data Master Role berhasil ditambahkan.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["AllMenus"] = AppMenus.AllMenus;
            return View(role);
        }

        // GET: MasterRole/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!User.HasMenuAccess(AppMenus.UserManagement))
            {
                return Forbid();
            }

            if (id == null) return NotFound();

            var role = await _context.MasterRoles.FindAsync(id);
            if (role == null) return NotFound();

            ViewData["AllMenus"] = AppMenus.AllMenus;
            return View(role);
        }

        // POST: MasterRole/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NamaRole,Deskripsi")] MasterRole role, string[] selectedMenus)
        {
            if (!User.HasMenuAccess(AppMenus.UserManagement))
            {
                return Forbid();
            }

            if (id != role.Id) return NotFound();

            role.AllowedMenus = selectedMenus != null && selectedMenus.Length > 0 
                ? string.Join(",", selectedMenus) 
                : "dashboard";

            if (ModelState.IsValid)
            {
                if (await _context.MasterRoles.AnyAsync(r => r.NamaRole == role.NamaRole && r.Id != id))
                {
                    ModelState.AddModelError("NamaRole", "Nama Role sudah digunakan oleh data lain.");
                    ViewData["AllMenus"] = AppMenus.AllMenus;
                    return View(role);
                }

                try
                {
                    _context.Update(role);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Data Master Role berhasil diperbarui.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.MasterRoles.AnyAsync(e => e.Id == role.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["AllMenus"] = AppMenus.AllMenus;
            return View(role);
        }

        // POST: MasterRole/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!User.HasMenuAccess(AppMenus.UserManagement))
            {
                return Forbid();
            }

            var role = await _context.MasterRoles
                .Include(r => r.UserList)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null) return NotFound();

            if (role.UserList.Any())
            {
                TempData["ErrorMessage"] = "Tidak dapat menghapus Role ini karena masih digunakan oleh beberapa pengguna.";
                return RedirectToAction(nameof(Index));
            }

            _context.MasterRoles.Remove(role);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Data Master Role berhasil dihapus.";
            return RedirectToAction(nameof(Index));
        }
    }
}
