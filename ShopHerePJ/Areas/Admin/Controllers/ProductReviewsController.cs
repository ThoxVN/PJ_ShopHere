using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopHerePJ.Areas.Admin.Models;
using ShopHerePJ.Data.Entities;

namespace ShopHerePJ.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductReviewsController : Controller
    {
        private readonly ShopHereContext _context;

        public ProductReviewsController(ShopHereContext context)
        {
            _context = context;
        }

        // GET: /Admin/ProductReviews?productId=5
        public async Task<IActionResult> Index(int? productId)
        {
            ViewData["Title"] = "Product Reviews";
            ViewBag.ProductId = productId;

            IQueryable<product_review> q = _context.product_reviews
                .Include(r => r.product)
                .Include(r => r.user)
                .AsNoTracking();

            if (productId.HasValue)
            {
                q = q.Where(r => r.product_id == productId.Value);

                var p = await _context.products.AsNoTracking().FirstOrDefaultAsync(x => x.id == productId.Value);
                ViewBag.ProductLabel = p != null ? $"{p.name} ({p.sku})" : $"Product #{productId.Value}";
            }

            var data = await q.OrderByDescending(r => r.id).ToListAsync();
            return View(data);
        }

        // POST: /Admin/ProductReviews/Search
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search(string search, int? productId)
        {
            ViewData["Title"] = "Product Reviews";
            ViewBag.Search = search;
            ViewBag.ProductId = productId;

            IQueryable<product_review> q = _context.product_reviews
                .Include(r => r.product)
                .Include(r => r.user)
                .AsNoTracking();

            if (productId.HasValue)
            {
                q = q.Where(r => r.product_id == productId.Value);
                var p = await _context.products.AsNoTracking().FirstOrDefaultAsync(x => x.id == productId.Value);
                ViewBag.ProductLabel = p != null ? $"{p.name} ({p.sku})" : $"Product #{productId.Value}";
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                q = q.Where(r =>
                    (r.title != null && r.title.Contains(search)) ||
                    (r.content != null && r.content.Contains(search)) ||
                    r.rating.ToString().Contains(search) ||
                    r.is_approved.ToString().Contains(search) ||
                    (r.product != null && (r.product.name.Contains(search) || r.product.sku.Contains(search))) ||
                    (r.user != null && (r.user.email.Contains(search) || (r.user.full_name != null && r.user.full_name.Contains(search))))
                );
            }

            var data = await q.OrderByDescending(r => r.id).ToListAsync();
            return View("Index", data);
        }

        // GET: /Admin/ProductReviews/ByProduct/5
        public IActionResult ByProduct(int productId)
        {
            return RedirectToAction(nameof(Index), new { productId });
        }

        // GET: /Admin/ProductReviews/Create?productId=5
        public async Task<IActionResult> Create(int? productId)
        {
            ViewData["Title"] = "Create Review";
            await LoadDropdowns(productId);

            return View(new ProductReviewUpsertVM
            {
                ProductId = productId ?? 0,
                Rating = 5,
                IsApproved = false
            });
        }

        // POST: /Admin/ProductReviews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductReviewUpsertVM vm)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdowns(vm.ProductId);
                return View(vm);
            }

            var entity = new product_review
            {
                product_id = vm.ProductId,
                user_id = vm.UserId,
                rating = vm.Rating,
                title = vm.Title?.Trim(),
                content = vm.Content?.Trim(),
                is_approved = vm.IsApproved,
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            _context.product_reviews.Add(entity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { productId = vm.ProductId });
        }

        // GET: /Admin/ProductReviews/Edit/10
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _context.product_reviews.FirstOrDefaultAsync(r => r.id == id);
            if (entity == null) return NotFound();

            ViewData["Title"] = "Edit Review";
            await LoadDropdowns(entity.product_id);

            var vm = new ProductReviewUpsertVM
            {
                Id = entity.id,
                ProductId = entity.product_id,
                UserId = entity.user_id,
                Rating = entity.rating,
                Title = entity.title,
                Content = entity.content,
                IsApproved = entity.is_approved
            };

            return View(vm);
        }

        // POST: /Admin/ProductReviews/Edit/10
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductReviewUpsertVM vm)
        {
            if (id != vm.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                await LoadDropdowns(vm.ProductId);
                return View(vm);
            }

            var entity = await _context.product_reviews.FirstOrDefaultAsync(r => r.id == id);
            if (entity == null) return NotFound();

            entity.product_id = vm.ProductId;
            entity.user_id = vm.UserId;
            entity.rating = vm.Rating;
            entity.title = vm.Title?.Trim();
            entity.content = vm.Content?.Trim();
            entity.is_approved = vm.IsApproved;
            entity.updated_at = DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { productId = vm.ProductId });
        }

        // GET: /Admin/ProductReviews/Delete/10
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.product_reviews
                .Include(r => r.product)
                .Include(r => r.user)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.id == id);

            if (entity == null) return NotFound();

            ViewData["Title"] = "Delete Review";
            return View(entity);
        }

        // POST: /Admin/ProductReviews/Delete/10
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entity = await _context.product_reviews.FirstOrDefaultAsync(r => r.id == id);
            if (entity == null) return NotFound();

            var pid = entity.product_id;

            _context.product_reviews.Remove(entity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { productId = pid });
        }

        private async Task LoadDropdowns(int? productId = null)
        {
            var products = await _context.products.AsNoTracking()
                .OrderBy(p => p.name)
                .Select(p => new { p.id, Label = p.name + " (" + p.sku + ")" })
                .ToListAsync();
            ViewBag.ProductSelect = new SelectList(products, "id", "Label", productId);

            var users = await _context.users.AsNoTracking()
                .OrderBy(u => u.email)
                .Select(u => new { u.userid, Label = u.email + (u.full_name != null ? " - " + u.full_name : "") })
                .ToListAsync();
            ViewBag.UserSelect = new SelectList(users, "userid", "Label");
        }
    }
}
