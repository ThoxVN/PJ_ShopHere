using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopHerePJ.Data.Entities;
using ShopHerePJ.Models;
using System.Security.Claims;

namespace ShopHerePJ.Controllers
{
    public class AuthController : Controller
    {
        private readonly ShopHereContext _context;
        public AuthController(ShopHereContext context) => _context = context;

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
            => View(new LoginVM { ReturnUrl = returnUrl });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var email = (vm.Email ?? "").Trim().ToLowerInvariant();
            var password = (vm.Password ?? "").Trim();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Vui lòng nhập email và mật khẩu.");
                return View(vm);
            }

            var u = await _context.users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.email.ToLower() == email);

            if (u == null)
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
                return View(vm);
            }

            // DB: active | locked
            if (!string.Equals(u.status, "active", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "Tài khoản đang bị khóa hoặc chưa kích hoạt.");
                return View(vm);
            }

            // Demo môn học: so sánh plain text (đúng theo yêu cầu của bạn)
            var ok = password == (u.password_hash ?? "");
            if (!ok)
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
                return View(vm);
            }
            
            var role = (u.role ?? "customer").Trim().ToLowerInvariant(); // admin|customer

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, u.userid.ToString()),
                new Claim(ClaimTypes.Email, u.email),
                new Claim(ClaimTypes.Name, string.IsNullOrWhiteSpace(u.full_name) ? u.email : u.full_name),
                new Claim(ClaimTypes.Role, role) // IMPORTANT: giữ đúng "admin" / "customer"
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties
                {
                    IsPersistent = vm.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });

            if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                return Redirect(vm.ReturnUrl);

            // ✅ Redirect theo role
            if (role == "admin")
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }
                

            // User/GUEST về trang store của bạn (đổi controller nếu bạn dùng StoreController)
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
    => View(new RegisterVM { ReturnUrl = returnUrl });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var email = (vm.Email ?? "").Trim().ToLowerInvariant();

            var exists = await _context.users.AsNoTracking()
                .AnyAsync(x => x.email.ToLower() == email);

            if (exists)
            {
                ModelState.AddModelError(nameof(vm.Email), "Email đã tồn tại.");
                return View(vm);
            }

            var now = DateTime.Now;

            // Demo môn học: lưu plain text theo yêu cầu (không khuyến nghị thực tế)
            var u = new user
            {
                email = email,
                password_hash = vm.Password,
                full_name = vm.FullName?.Trim(),
                phone = vm.Phone?.Trim(),
                role = "customer",
                status = "active",
                created_at = now,
                updated_at = now
            };

            _context.users.Add(u);
            await _context.SaveChangesAsync();

            TempData["RegisterOk"] = "Đăng ký thành công. Vui lòng đăng nhập.";

            if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                return RedirectToAction("Login", new { returnUrl = vm.ReturnUrl });

            return RedirectToAction("Login");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Denied() => View();
    }
}
