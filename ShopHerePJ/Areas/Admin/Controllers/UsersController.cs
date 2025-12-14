using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopHerePJ.Areas.Admin.Models;
using ShopHerePJ.Data.Entities; // ShopHereContext
using ShopHerePJ.Models;       // user (đúng theo code bạn gửi)

namespace ShopHerePJ.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UsersController : Controller
    {
        private readonly ShopHereContext _context;
        private readonly PasswordHasher<user> _hasher = new PasswordHasher<user>();

        public UsersController(ShopHereContext context)
        {
            _context = context;
        }

        // GET: /Admin/Users
        public async Task<IActionResult> Index()
        {
            var users = await _context.users
                .AsNoTracking()
                .OrderByDescending(x => x.userid)
                .ToListAsync();

            return View(users);
        }

        // POST: /Admin/Users/Search
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search(string search)
        {
            ViewBag.Search = search;

            IQueryable<user> q = _context.users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                q = q.Where(x =>
                    x.email.Contains(search) ||
                    (x.full_name != null && x.full_name.Contains(search)) ||
                    (x.phone != null && x.phone.Contains(search)) ||
                    x.role.Contains(search) ||
                    x.status.Contains(search)
                );
            }

            var users = await q.OrderByDescending(x => x.userid).ToListAsync();
            return View("Index", users);
        }

        // GET: /Admin/Users/Create
        public IActionResult Create()
        {
            return View(new UserUpsertVM
            {
                Role = "Customer",
                Status = "Active"
            });
        }

        // POST: /Admin/Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserUpsertVM vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Password))
                ModelState.AddModelError(nameof(vm.Password), "Password là bắt buộc.");

            if (!ModelState.IsValid) return View(vm);

            var entity = new user
            {
                email = vm.Email.Trim(),
                full_name = vm.FullName?.Trim(),
                phone = vm.Phone?.Trim(),
                role = vm.Role.Trim(),
                status = vm.Status.Trim(),
                created_at = DateTime.Now,
                updated_at = DateTime.Now,
            };

            entity.password_hash = _hasher.HashPassword(entity, vm.Password!);

            _context.users.Add(entity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Users/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _context.users.FirstOrDefaultAsync(x => x.userid == id);
            if (entity == null) return NotFound();

            var vm = new UserUpsertVM
            {
                UserId = entity.userid,
                Email = entity.email,
                FullName = entity.full_name,
                Phone = entity.phone,
                Role = entity.role,
                Status = entity.status
            };

            return View(vm);
        }

        // POST: /Admin/Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserUpsertVM vm)
        {
            if (id != vm.UserId) return BadRequest();

            if (!ModelState.IsValid) return View(vm);

            var entity = await _context.users.FirstOrDefaultAsync(x => x.userid == id);
            if (entity == null) return NotFound();

            entity.email = vm.Email.Trim();
            entity.full_name = vm.FullName?.Trim();
            entity.phone = vm.Phone?.Trim();
            entity.role = vm.Role.Trim();
            entity.status = vm.Status.Trim();
            entity.updated_at = DateTime.Now;

            // Nếu có nhập Password thì update hash
            if (!string.IsNullOrWhiteSpace(vm.Password))
            {
                entity.password_hash = _hasher.HashPassword(entity, vm.Password.Trim());
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Users/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.userid == id);

            if (entity == null) return NotFound();
            return View(entity);
        }

        // POST: /Admin/Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entity = await _context.users.FirstOrDefaultAsync(x => x.userid == id);
            if (entity == null) return NotFound();

            _context.users.Remove(entity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
