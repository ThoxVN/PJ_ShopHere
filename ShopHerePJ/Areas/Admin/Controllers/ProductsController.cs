using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopHerePJ.Areas.Admin.Models;
using ShopHerePJ.Data.Entities;

namespace ShopHerePJ.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly ShopHereContext _context;

        public ProductsController(ShopHereContext context)
        {
            _context = context;
        }

        // GET: /Admin/Products
        public async Task<IActionResult> Index()
        {
            var data = await _context.products
                .Include(p => p.category)
                .AsNoTracking()
                .OrderByDescending(p => p.id)
                .ToListAsync();

            return View(data);
        }

        // POST: /Admin/Products/Search
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search(string search)
        {
            ViewBag.Search = search;

            IQueryable<product> q = _context.products
                .Include(p => p.category)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                q = q.Where(p =>
                    p.sku.Contains(search) ||
                    p.name.Contains(search) ||
                    (p.material != null && p.material.Contains(search)) ||
                    (p.category != null && p.category.name.Contains(search))
                );
            }

            var data = await q.OrderByDescending(p => p.id).ToListAsync();
            return View("Index", data);
        }

        // GET: /Admin/Products/Create
        public async Task<IActionResult> Create()
        {
            await LoadCategoriesDropdown();
            return View(new ProductUpsertVM { IsActive = true });
        }

        // POST: /Admin/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductUpsertVM vm)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoriesDropdown();
                return View(vm);
            }

            var entity = new product
            {
                category_id = vm.CategoryId,
                sku = vm.Sku.Trim(),
                name = vm.Name.Trim(),
                description_html = vm.DescriptionHtml,
                material = vm.Material?.Trim(),
                is_active = vm.IsActive,
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            _context.products.Add(entity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Edit), new { id = entity.id });
        }

        // GET: /Admin/Products/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _context.products.FirstOrDefaultAsync(p => p.id == id);
            if (entity == null) return NotFound();

            await LoadCategoriesDropdown();

            // lấy ảnh primary
            var img = await _context.images.AsNoTracking()
                .Where(x => x.object_type == "product" && x.object_id == id && x.is_primary)
                .OrderByDescending(x => x.id)
                .FirstOrDefaultAsync();
            ViewBag.ProductImageId = img?.id;

            var vm = new ProductUpsertVM
            {
                Id = entity.id,
                CategoryId = entity.category_id,
                Sku = entity.sku,
                Name = entity.name,
                DescriptionHtml = entity.description_html,
                Material = entity.material,
                IsActive = entity.is_active
            };

            return View(vm);
        }

        // POST: /Admin/Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductUpsertVM vm)
        {
            if (id != vm.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                await LoadCategoriesDropdown();
                return View(vm);
            }

            var entity = await _context.products.FirstOrDefaultAsync(p => p.id == id);
            if (entity == null) return NotFound();

            entity.category_id = vm.CategoryId;
            entity.sku = vm.Sku.Trim();
            entity.name = vm.Name.Trim();
            entity.description_html = vm.DescriptionHtml;
            entity.material = vm.Material?.Trim();
            entity.is_active = vm.IsActive;
            entity.updated_at = DateTime.Now;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Edit), new { id });
        }

        // GET: /Admin/Products/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.products
                .Include(p => p.category)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.id == id);

            if (entity == null) return NotFound();
            return View(entity);
        }

        // POST: /Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entity = await _context.products.FirstOrDefaultAsync(p => p.id == id);
            if (entity == null) return NotFound();

            _context.products.Remove(entity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadCategoriesDropdown()
        {
            var cats = await _context.categories
                .AsNoTracking()
                .OrderBy(c => c.name)
                .Select(c => new { c.id, Label = c.name + " (" + c.slug + ")" })
                .ToListAsync();

            ViewBag.CategorySelect = new SelectList(cats, "id", "Label");
        }
    }
}
