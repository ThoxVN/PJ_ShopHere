using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Generators;
using ShopHerePJ.Data.Entities;
using ShopHerePJ.Models;
using System.Security.Claims;

namespace ShopHerePJ.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly ShopHereContext _context;
        public AccountController(ShopHereContext context) => _context = context;

        private int CurrentUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(idStr!);
        }

        // GET: /Account/Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var uid = CurrentUserId();
            var u = await _context.users.AsNoTracking().FirstOrDefaultAsync(x => x.userid == uid);
            if (u == null) return RedirectToAction("Logout", "Auth");

            var vm = new ProfileVM
            {
                UserId = u.userid,
                Email = u.email,
                FullName = u.full_name,
                Phone = u.phone
            };

            return View(vm);
        }

        // POST: /Account/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileVM vm)
        {
            var uid = CurrentUserId();
            if (vm.UserId != uid) return Forbid();

            // Nếu user nhập NewPassword thì bắt buộc CurrentPassword
            if (!string.IsNullOrWhiteSpace(vm.NewPassword) && string.IsNullOrWhiteSpace(vm.CurrentPassword))
                ModelState.AddModelError(nameof(vm.CurrentPassword), "Vui lòng nhập mật khẩu hiện tại để đổi mật khẩu.");

            if (!ModelState.IsValid) return View(vm);

            var u = await _context.users.FirstOrDefaultAsync(x => x.userid == uid);
            if (u == null) return RedirectToAction("Logout", "Auth");

            // Update basic info
            u.full_name = vm.FullName?.Trim();
            u.phone = vm.Phone?.Trim();
            u.updated_at = DateTime.Now;

            // Optional: change password (PLAINTEXT - đơn giản cho môn học)
            if (!string.IsNullOrWhiteSpace(vm.NewPassword))
            {
                // bắt buộc nhập mật khẩu hiện tại
                if (string.IsNullOrWhiteSpace(vm.CurrentPassword))
                {
                    ModelState.AddModelError(nameof(vm.CurrentPassword), "Vui lòng nhập mật khẩu hiện tại.");
                    return View(vm);
                }

                // check mật khẩu hiện tại
                var ok = vm.CurrentPassword == u.password_hash;
                if (!ok)
                {
                    ModelState.AddModelError(nameof(vm.CurrentPassword), "Mật khẩu hiện tại không đúng.");
                    return View(vm);
                }

                // set mật khẩu mới
                u.password_hash = vm.NewPassword;
                u.updated_at = DateTime.Now;
            }


            await _context.SaveChangesAsync();
            TempData["ProfileSuccess"] = "Cập nhật hồ sơ thành công.";
            return RedirectToAction(nameof(Profile));
        }

        // GET: /Account/Orders
        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            var uid = CurrentUserId();

            var orders = await _context.orders
                .AsNoTracking()
                .Where(o => o.user_id == uid)
                .OrderByDescending(o => o.created_at)
                .ToListAsync();

            return View(orders);
        }

        // GET: /Account/OrderDetails/5
        [HttpGet]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var uid = CurrentUserId();

            var order = await _context.orders
                .AsNoTracking()
                .Include(o => o.order_items)
                .FirstOrDefaultAsync(o => o.id == id);

            if (order == null) return NotFound();
            if (order.user_id != uid) return Forbid();

            return View(order);
        }

        // GET: /Account/Addresses
        [HttpGet]
        public async Task<IActionResult> Addresses()
        {
            var uid = CurrentUserId();

            var addresses = await _context.addresses
                .AsNoTracking()
                .Where(a => a.user_id == uid && a.type == "Shipping")
                .OrderByDescending(a => a.is_default)
                .ThenByDescending(a => a.updated_at)
                .ToListAsync();

            return View(addresses);
        }

        // POST: /Account/SetDefaultAddress/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetDefaultAddress(int id)
        {
            var uid = CurrentUserId();

            var addr = await _context.addresses.FirstOrDefaultAsync(a => a.id == id && a.user_id == uid);
            if (addr == null) return NotFound();

            // Unset all default (shipping only)
            var all = await _context.addresses
                .Where(a => a.user_id == uid && a.type == "Shipping")
                .ToListAsync();

            foreach (var a in all) a.is_default = false;

            addr.is_default = true;
            addr.updated_at = DateTime.Now;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Addresses));
        }      
        

        // GET: /Account/AddressCreate
        [HttpGet]
        public IActionResult AddressCreate()
        {
            return View(new AccAddressUpsert
            {
                Type = "Shipping",
                Country = "Vietnam",
                IsDefault = false
            });
        }

        // POST: /Account/AddressCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddressCreate(AccAddressUpsert vm)
        {
            var uid = CurrentUserId();
            if (!ModelState.IsValid) return View(vm);

            var now = DateTime.Now;

            var a = new address
            {
                user_id = uid,
                type = vm.Type ?? "Shipping",
                recipient_name = vm.RecipientName?.Trim(),
                phone = vm.Phone?.Trim(),
                street = vm.Street?.Trim(),
                ward = vm.Ward?.Trim(),
                district = vm.District?.Trim(),
                city = vm.City?.Trim(),
                postal_code = vm.PostalCode?.Trim(),
                country = vm.Country?.Trim() ?? "Vietnam",
                is_default = vm.IsDefault,
                created_at = now,
                updated_at = now
            };

            _context.addresses.Add(a);
            await _context.SaveChangesAsync();

            // đảm bảo chỉ 1 default
            if (vm.IsDefault)
                await MakeOnlyOneDefault(uid, a.id, a.type);

            return RedirectToAction(nameof(Addresses));
        }

        // GET: /Account/AddressEdit/5
        [HttpGet]
        public async Task<IActionResult> AddressEdit(int id)
        {
            var uid = CurrentUserId();
            var a = await _context.addresses.AsNoTracking()
                .FirstOrDefaultAsync(x => x.id == id && x.user_id == uid);

            if (a == null) return NotFound();

            var vm = new AccAddressUpsert
            {
                Id = a.id,
                Type = a.type,
                RecipientName = a.recipient_name,
                Phone = a.phone,
                Street = a.street,
                Ward = a.ward,
                District = a.district,
                City = a.city,
                PostalCode = a.postal_code,
                Country = a.country,
                IsDefault = a.is_default
            };

            return View(vm);
        }

        // POST: /Account/AddressEdit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddressEdit(AccAddressUpsert vm)
        {
            var uid = CurrentUserId();
            if (!ModelState.IsValid) return View(vm);

            var a = await _context.addresses
                .FirstOrDefaultAsync(x => x.id == vm.Id && x.user_id == uid);

            if (a == null) return NotFound();

            a.type = vm.Type ?? a.type;
            a.recipient_name = vm.RecipientName?.Trim();
            a.phone = vm.Phone?.Trim();
            a.street = vm.Street?.Trim();
            a.ward = vm.Ward?.Trim();
            a.district = vm.District?.Trim();
            a.city = vm.City?.Trim();
            a.postal_code = vm.PostalCode?.Trim();
            a.country = vm.Country?.Trim() ?? a.country;
            a.is_default = vm.IsDefault;
            a.updated_at = DateTime.Now;

            await _context.SaveChangesAsync();

            // đảm bảo chỉ 1 default
            if (vm.IsDefault)
                await MakeOnlyOneDefault(uid, a.id, a.type);

            return RedirectToAction(nameof(Addresses));
        }

        // helper: đảm bảo chỉ 1 default theo type (Shipping)
        private async Task MakeOnlyOneDefault(int uid, int keepId, string type)
        {
            var others = await _context.addresses
                .Where(x => x.user_id == uid && x.type == type && x.id != keepId && x.is_default)
                .ToListAsync();

            foreach (var x in others) x.is_default = false;
            await _context.SaveChangesAsync();
        }    


    }
}
