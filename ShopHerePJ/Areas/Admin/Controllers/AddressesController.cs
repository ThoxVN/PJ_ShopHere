using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopHerePJ.Areas.Admin.Models;
using ShopHerePJ.Data.Entities;
using ShopHerePJ.Models; // user
// address nằm ở ShopHerePJ.Data.Entities

namespace ShopHerePJ.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AddressesController : Controller
    {
        private readonly ShopHereContext _context;

        public AddressesController(ShopHereContext context)
        {
            _context = context;
        }

        // GET: /Admin/Addresses
        public async Task<IActionResult> Index(int? userId)
        {
            ViewData["Title"] = "Addresses";
            ViewBag.UserId = userId;

            IQueryable<address> q = _context.addresses
                .Include(a => a.user)
                .AsNoTracking();

            if (userId.HasValue)
            {
                q = q.Where(a => a.user_id == userId.Value);

                var u = await _context.users.AsNoTracking().FirstOrDefaultAsync(x => x.userid == userId.Value);
                ViewBag.UserLabel = u != null ? $"{u.email} ({u.full_name})" : $"User #{userId.Value}";
            }

            var data = await q.OrderByDescending(a => a.id).ToListAsync();
            return View(data);
        }

        // POST: /Admin/Addresses/Search
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search(string search, int? userId)
        {
            ViewData["Title"] = "Addresses";
            ViewBag.Search = search;
            ViewBag.UserId = userId;

            IQueryable<address> q = _context.addresses
                .Include(a => a.user)
                .AsNoTracking();

            if (userId.HasValue)
            {
                q = q.Where(a => a.user_id == userId.Value);

                var u = await _context.users.AsNoTracking().FirstOrDefaultAsync(x => x.userid == userId.Value);
                ViewBag.UserLabel = u != null ? $"{u.email} ({u.full_name})" : $"User #{userId.Value}";
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                q = q.Where(a =>
                    a.type.Contains(search) ||
                    (a.recipient_name != null && a.recipient_name.Contains(search)) ||
                    (a.phone != null && a.phone.Contains(search)) ||
                    (a.street != null && a.street.Contains(search)) ||
                    (a.ward != null && a.ward.Contains(search)) ||
                    (a.district != null && a.district.Contains(search)) ||
                    (a.city != null && a.city.Contains(search)) ||
                    (a.postal_code != null && a.postal_code.Contains(search)) ||
                    a.country.Contains(search) ||
                    (a.user != null && (
                        a.user.email.Contains(search) ||
                        (a.user.full_name != null && a.user.full_name.Contains(search))
                    ))
                );
            }

            var data = await q.OrderByDescending(a => a.id).ToListAsync();
            return View("Index", data);
        }

        // GET: /Admin/Addresses/ByUser/5
        public IActionResult ByUser(int userId)
        {
            return RedirectToAction(nameof(Index), new { userId = userId });
        }


        // GET: /Admin/Addresses/Create?userId=5
        public async Task<IActionResult> Create(int? userId)
        {
            ViewData["Title"] = "Create Address";

            await LoadUsersDropdown();
            var vm = new AddressUpsertVM
            {
                UserId = userId,
                Country = "Vietnam",
                Type = "Shipping",
                IsDefault = false
            };
            return View(vm);
        }

        // POST: /Admin/Addresses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddressUpsertVM vm)
        {
            if (!ModelState.IsValid)
            {
                await LoadUsersDropdown();
                return View(vm);
            }

            var entity = new address
            {
                user_id = vm.UserId,
                type = vm.Type.Trim(),
                recipient_name = vm.RecipientName?.Trim(),
                phone = vm.Phone?.Trim(),
                street = vm.Street?.Trim(),
                ward = vm.Ward?.Trim(),
                district = vm.District?.Trim(),
                city = vm.City?.Trim(),
                postal_code = vm.PostalCode?.Trim(),
                country = vm.Country.Trim(),
                is_default = vm.IsDefault,
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            // Nếu set default => clear default của address khác cùng user
            if (entity.is_default && entity.user_id.HasValue)
            {
                var others = await _context.addresses
                    .Where(a => a.user_id == entity.user_id.Value)
                    .ToListAsync();

                foreach (var a in others) a.is_default = false;
            }

            _context.addresses.Add(entity);
            await _context.SaveChangesAsync();

            if (vm.UserId.HasValue) return RedirectToAction(nameof(ByUser), new { userId = vm.UserId.Value });
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Addresses/Edit/10
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _context.addresses.FirstOrDefaultAsync(a => a.id == id);
            if (entity == null) return NotFound();

            ViewData["Title"] = "Edit Address";
            await LoadUsersDropdown();

            var vm = new AddressUpsertVM
            {
                Id = entity.id,
                UserId = entity.user_id,
                Type = entity.type,
                RecipientName = entity.recipient_name,
                Phone = entity.phone,
                Street = entity.street,
                Ward = entity.ward,
                District = entity.district,
                City = entity.city,
                PostalCode = entity.postal_code,
                Country = entity.country,
                IsDefault = entity.is_default
            };

            return View(vm);
        }

        // POST: /Admin/Addresses/Edit/10
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AddressUpsertVM vm)
        {
            if (id != vm.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                await LoadUsersDropdown();
                return View(vm);
            }

            var entity = await _context.addresses.FirstOrDefaultAsync(a => a.id == id);
            if (entity == null) return NotFound();

            entity.user_id = vm.UserId;
            entity.type = vm.Type.Trim();
            entity.recipient_name = vm.RecipientName?.Trim();
            entity.phone = vm.Phone?.Trim();
            entity.street = vm.Street?.Trim();
            entity.ward = vm.Ward?.Trim();
            entity.district = vm.District?.Trim();
            entity.city = vm.City?.Trim();
            entity.postal_code = vm.PostalCode?.Trim();
            entity.country = vm.Country.Trim();
            entity.is_default = vm.IsDefault;
            entity.updated_at = DateTime.Now;

            if (entity.is_default && entity.user_id.HasValue)
            {
                var others = await _context.addresses
                    .Where(a => a.user_id == entity.user_id.Value && a.id != entity.id)
                    .ToListAsync();

                foreach (var a in others) a.is_default = false;
            }

            await _context.SaveChangesAsync();

            if (vm.UserId.HasValue) return RedirectToAction(nameof(ByUser), new { userId = vm.UserId.Value });
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Addresses/Delete/10
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.addresses
                .Include(a => a.user)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.id == id);

            if (entity == null) return NotFound();

            ViewData["Title"] = "Delete Address";
            return View(entity);
        }

        // POST: /Admin/Addresses/Delete/10
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entity = await _context.addresses.FirstOrDefaultAsync(a => a.id == id);
            if (entity == null) return NotFound();

            var userId = entity.user_id;

            _context.addresses.Remove(entity);
            await _context.SaveChangesAsync();

            if (userId.HasValue) return RedirectToAction(nameof(ByUser), new { userId = userId.Value });
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadUsersDropdown()
        {
            var users = await _context.users
                .AsNoTracking()
                .OrderBy(u => u.email)
                .Select(u => new { u.userid, Label = u.email + (u.full_name != null ? " - " + u.full_name : "") })
                .ToListAsync();

            ViewBag.UserSelect = new SelectList(users, "userid", "Label");
        }
    }
}
