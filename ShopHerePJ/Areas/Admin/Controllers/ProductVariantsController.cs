using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopHerePJ.Areas.Admin.Models;
using ShopHerePJ.Data.Entities;

namespace ShopHerePJ.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductVariantsController : Controller
    {
        private readonly ShopHereContext _context;

        public ProductVariantsController(ShopHereContext context)
        {
            _context = context;
        }

        // GET: /Admin/ProductVariants?productId=5
        public async Task<IActionResult> Index(int? productId)
        {
            ViewBag.ProductId = productId;

            IQueryable<product_variant> q = _context.product_variants
                .Include(v => v.product).ThenInclude(p => p.category)
                .AsNoTracking();

            if (productId.HasValue)
            {
                q = q.Where(v => v.product_id == productId.Value);

                var p = await _context.products.AsNoTracking().FirstOrDefaultAsync(x => x.id == productId.Value);
                ViewBag.ProductLabel = p != null ? $"{p.name} ({p.sku})" : $"Product #{productId.Value}";
            }

            var data = await q.OrderByDescending(v => v.id).ToListAsync();
            return View(data);
        }

        // POST: /Admin/ProductVariants/Search
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search(string search, int? productId)
        {
            ViewBag.Search = search;
            ViewBag.ProductId = productId;

            IQueryable<product_variant> q = _context.product_variants
                .Include(v => v.product).ThenInclude(p => p.category)
                .AsNoTracking();

            if (productId.HasValue)
            {
                q = q.Where(v => v.product_id == productId.Value);
                var p = await _context.products.AsNoTracking().FirstOrDefaultAsync(x => x.id == productId.Value);
                ViewBag.ProductLabel = p != null ? $"{p.name} ({p.sku})" : $"Product #{productId.Value}";
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                q = q.Where(v =>
                    v.sku.Contains(search) ||
                    v.size.Contains(search) ||
                    v.color.Contains(search) ||
                    (v.name_extension != null && v.name_extension.Contains(search)) ||
                    (v.product != null && (v.product.name.Contains(search) || v.product.sku.Contains(search)))
                );
            }

            var data = await q.OrderByDescending(v => v.id).ToListAsync();
            return View("Index", data);
        }

        // GET: /Admin/ProductVariants/ByProduct/5
        public IActionResult ByProduct(int productId)
        {
            return RedirectToAction(nameof(Index), new { productId });
        }

        // GET: /Admin/ProductVariants/Create?productId=5
        public async Task<IActionResult> Create(int? productId)
        {
            await LoadProductsDropdown(productId);
            return View(new ProductVariantUpsertVM
            {
                ProductId = productId ?? 0,
                IsActive = true
            });
        }

        // POST: /Admin/ProductVariants/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductVariantUpsertVM vm)
        {
            if (!ModelState.IsValid)
            {
                await LoadProductsDropdown(vm.ProductId);
                return View(vm);
            }

            var entity = new product_variant
            {
                product_id = vm.ProductId,
                sku = vm.Sku.Trim(),
                name_extension = vm.NameExtension?.Trim(),
                size = vm.Size.Trim(),
                color = vm.Color.Trim(),
                price_modifier = vm.PriceModifier,
                is_active = vm.IsActive,
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            _context.product_variants.Add(entity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Edit), new { id = entity.id });
        }

        // GET: /Admin/ProductVariants/Edit/10
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _context.product_variants
                .Include(v => v.product)
                .FirstOrDefaultAsync(v => v.id == id);

            if (entity == null) return NotFound();

            await LoadProductsDropdown(entity.product_id);

            // ảnh primary theo variant
            var img = await _context.images.AsNoTracking()
                .Where(x => x.object_type == "product_variant" && x.object_id == id && x.is_primary)
                .OrderByDescending(x => x.id)
                .FirstOrDefaultAsync();
            ViewBag.VariantImageId = img?.id;

            var vm = new ProductVariantUpsertVM
            {
                Id = entity.id,
                ProductId = entity.product_id,
                Sku = entity.sku,
                NameExtension = entity.name_extension,
                Size = entity.size,
                Color = entity.color,
                PriceModifier = entity.price_modifier,
                IsActive = entity.is_active
            };

            return View(vm);
        }

        // POST: /Admin/ProductVariants/Edit/10
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductVariantUpsertVM vm)
        {
            if (id != vm.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                await LoadProductsDropdown(vm.ProductId);
                return View(vm);
            }

            var entity = await _context.product_variants.FirstOrDefaultAsync(v => v.id == id);
            if (entity == null) return NotFound();

            entity.product_id = vm.ProductId;
            entity.sku = vm.Sku.Trim();
            entity.name_extension = vm.NameExtension?.Trim();
            entity.size = vm.Size.Trim();
            entity.color = vm.Color.Trim();
            entity.price_modifier = vm.PriceModifier;
            entity.is_active = vm.IsActive;
            entity.updated_at = DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Edit), new { id });
        }

        // GET: /Admin/ProductVariants/Delete/10
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.product_variants
                .Include(v => v.product)
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.id == id);

            if (entity == null) return NotFound();
            return View(entity);
        }

        // POST: /Admin/ProductVariants/Delete/10
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entity = await _context.product_variants.FirstOrDefaultAsync(v => v.id == id);
            if (entity == null) return NotFound();

            var pid = entity.product_id;

            _context.product_variants.Remove(entity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { productId = pid });
        }

        private async Task LoadProductsDropdown(int? selectedProductId = null)
        {
            var products = await _context.products.AsNoTracking()
                .OrderBy(p => p.name)
                .Select(p => new { p.id, Label = p.name + " (" + p.sku + ")" })
                .ToListAsync();

            ViewBag.ProductSelect = new SelectList(products, "id", "Label", selectedProductId);
        }
    }
}
