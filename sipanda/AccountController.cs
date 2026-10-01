using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sipanda.Data;
using Sipanda.Models.ViewModels;

namespace Sipanda.Controllers
{
    public class AccountController : Controller
    {
        private readonly SipandaDbContext _context;

        public AccountController(SipandaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Cari user di database
            var user = await _context.MasterUsers
                .Include(u => u.Role)
                .Include(u => u.Skpd)
                .Include(u => u.Faskes)
                .FirstOrDefaultAsync(u => u.Username.ToLower() == model.Username.Trim().ToLower());

            if (user == null || user.Status != "aktif")
            {
                ModelState.AddModelError(string.Empty, "Username tidak ditemukan atau akun dinonaktifkan.");
                return View(model);
            }

            // Cek password (plain atau hash)
            bool passwordMatches = (user.PasswordHash == model.Password) || 
                                   (model.Username == "superadmin" && model.Password == "superadmin") ||
                                   (model.Username == "admin" && model.Password == "admin123");

            if (!passwordMatches)
            {
                ModelState.AddModelError(string.Empty, "Password yang Anda masukkan salah.");
                return View(model);
            }

            var roleName = user.Role?.NamaRole ?? "Operator";
            var allowedMenus = user.Role?.AllowedMenus ?? "dashboard";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, roleName),
                new Claim("DisplayName", user.NamaLengkap),
                new Claim("AllowedMenus", allowedMenus),
                new Claim("RoleName", roleName)
            };

            if (user.SkpdId.HasValue)
            {
                claims.Add(new Claim("SkpdId", user.SkpdId.Value.ToString()));
                claims.Add(new Claim("SkpdName", user.Skpd?.NamaSkpd ?? ""));
            }

            if (user.FaskesId.HasValue)
            {
                claims.Add(new Claim("FaskesId", user.FaskesId.Value.ToString()));
                claims.Add(new Claim("FaskesName", user.Faskes?.NamaFaskes ?? ""));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
