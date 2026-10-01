using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sipanda.Data;
using Sipanda.Helpers;
using Sipanda.Models.Entities;

namespace Sipanda.Controllers
{
    [Authorize]
    public class MasterUserController : Controller
    {
        private readonly SipandaDbContext _context;

        public MasterUserController(SipandaDbContext context)
        {
            _context = context;
        }

        // GET: MasterUser
        public async Task<IActionResult> Index(string? keyword, int? roleId)
        {
            if (!User.HasMenuAccess(AppMenus.UserManagement))
            {
                return Forbid();
            }

            var query = _context.MasterUsers
                .Include(u => u.Role)
                .Include(u => u.Skpd)
                .Include(u => u.Faskes)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                query = query.Where(u => u.Username.ToLower().Contains(kw) || 
                                         u.NamaLengkap.ToLower().Contains(kw) || 
                                         (u.Email != null && u.Email.ToLower().Contains(kw)));
            }

            if (roleId.HasValue && roleId.Value > 0)
            {
                query = query.Where(u => u.RoleId == roleId.Value);
            }

            var users = await query.OrderBy(u => u.Id).ToListAsync();

            ViewData["Keyword"] = keyword;
            ViewData["RoleId"] = roleId;
            ViewData["RoleList"] = new SelectList(await _context.MasterRoles.OrderBy(r => r.NamaRole).ToListAsync(), "Id", "NamaRole", roleId);

            return View(users);
        }

        // GET: MasterUser/Create
        public async Task<IActionResult> Create()
        {
            if (!User.HasMenuAccess(AppMenus.UserManagement))
            {
                return Forbid();
            }

            await PopulateDropdowns();
            return View(new MasterUser { Status = "aktif" });
        }

        // POST: MasterUser/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Username,NamaLengkap,Email,PasswordHash,RoleId,SkpdId,FaskesId,Status")] MasterUser masterUser)
        {
            if (!User.HasMenuAccess(AppMenus.UserManagement))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                if (await _context.MasterUsers.AnyAsync(u => u.Username.ToLower() == masterUser.Username.Trim().ToLower()))
                {
                    ModelState.AddModelError("Username", "Username sudah terdaftar.");
                    await PopulateDropdowns(masterUser.RoleId, masterUser.SkpdId, masterUser.FaskesId);
                    return View(masterUser);
                }

                masterUser.CreatedAt = DateTime.UtcNow;
                _context.Add(masterUser);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Pengguna baru berhasil ditambahkan.";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdowns(masterUser.RoleId, masterUser.SkpdId, masterUser.FaskesId);
            return View(masterUser);
        }

        // GET: MasterUser/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!User.HasMenuAccess(AppMenus.UserManagement))
            {
                return Forbid();
            }

            if (id == null) return NotFound();

            var masterUser = await _context.MasterUsers.FindAsync(id);
            if (masterUser == null) return NotFound();

            await PopulateDropdowns(masterUser.RoleId, masterUser.SkpdId, masterUser.FaskesId);
            return View(masterUser);
        }

        // POST: MasterUser/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Username,NamaLengkap,Email,PasswordHash,RoleId,SkpdId,FaskesId,Status")] MasterUser masterUser)
        {
            if (!User.HasMenuAccess(AppMenus.UserManagement))
            {
                return Forbid();
            }

            if (id != masterUser.Id) return NotFound();

            if (ModelState.IsValid)
            {
                if (await _context.MasterUsers.AnyAsync(u => u.Username.ToLower() == masterUser.Username.Trim().ToLower() && u.Id != id))
                {
                    ModelState.AddModelError("Username", "Username sudah digunakan pengguna lain.");
                    await PopulateDropdowns(masterUser.RoleId, masterUser.SkpdId, masterUser.FaskesId);
                    return View(masterUser);
                }

                try
                {
                    _context.Update(masterUser);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Data Pengguna berhasil diperbarui.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.MasterUsers.AnyAsync(e => e.Id == masterUser.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdowns(masterUser.RoleId, masterUser.SkpdId, masterUser.FaskesId);
            return View(masterUser);
        }

        // POST: MasterUser/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!User.HasMenuAccess(AppMenus.UserManagement))
            {
                return Forbid();
            }

            var masterUser = await _context.MasterUsers.FindAsync(id);
            if (masterUser == null) return NotFound();

            if (User.Identity?.Name?.ToLower() == masterUser.Username.ToLower())
            {
                TempData["ErrorMessage"] = "Anda tidak dapat menghapus akun Anda sendiri yang sedang aktif digunakan.";
                return RedirectToAction(nameof(Index));
            }

            _context.MasterUsers.Remove(masterUser);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Data Pengguna berhasil dihapus.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdowns(int? roleId = null, int? skpdId = null, int? faskesId = null)
        {
            ViewData["RoleId"] = new SelectList(
                await _context.MasterRoles.OrderBy(r => r.NamaRole).ToListAsync(),
                "Id", "NamaRole", roleId);

            ViewData["SkpdId"] = new SelectList(
                await _context.MasterSkpd.OrderBy(s => s.NamaSkpd).ToListAsync(),
                "Id", "NamaSkpd", skpdId);

            ViewData["FaskesId"] = new SelectList(
                await _context.MasterFaskes.OrderBy(f => f.NamaFaskes).ToListAsync(),
                "Id", "NamaFaskes", faskesId);
        }
    }
}
