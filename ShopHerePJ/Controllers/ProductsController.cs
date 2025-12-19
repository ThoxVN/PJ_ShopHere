using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopHerePJ.Data.Entities;
using ShopHerePJ.Models;

namespace ShopHerePJ.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ShopHereContext _context;
        public ProductsController(ShopHereContext context) => _context = context;

        // GET: /Products/Shop?q=&categoryId=&minRating=
        [HttpGet]
        public async Task<IActionResult> Shop(string? q, int? categoryId, double? minRating)
        {
            q = q?.Trim();

            var vm = new ShopListVM
            {
                Q = q,
                CategoryId = categoryId,
                MinRating = minRating
            };

            // Dropdown category
            vm.Categories = await _context.categories.AsNoTracking()
                .Where(c => c.is_active)
                .OrderBy(c => c.name)
                .Select(c => new CategoryOpt { Id = c.id, Name = c.name })
                .ToListAsync();

            // 1) Product base list (filter Name/Category)
            var pQuery = _context.products.AsNoTracking()
                .Include(p => p.category)
                .Where(p => p.is_active);

            if (!string.IsNullOrWhiteSpace(q))
                pQuery = pQuery.Where(p => p.name.Contains(q) || p.sku.Contains(q));

            if (categoryId.HasValue)
                pQuery = pQuery.Where(p => p.category_id == categoryId.Value);

            var products = await pQuery
                .OrderByDescending(p => p.created_at)
                .Select(p => new
                {
                    p.id,
                    p.sku,
                    p.name,
                    CategoryName = p.category != null ? p.category.name : "Uncategorized"
                })
                .ToListAsync();

            var ids = products.Select(x => x.id).ToList();

            if (!ids.Any())
            {
                vm.Items = new List<ProductCardVM>();
                return View(vm);
            }

            // 2) Rating map (approved)
            var ratingRows = await _context.product_reviews.AsNoTracking()
                .Where(r => r.is_approved && ids.Contains(r.product_id))
                .GroupBy(r => r.product_id)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Avg = g.Average(x => (double)x.rating),
                    Cnt = g.Count()
                })
                .ToListAsync();

            var ratingMap = ratingRows.ToDictionary(x => x.ProductId, x => x);

            // 3) Primary image map (latest primary)
            var imgRows = await _context.images.AsNoTracking()
                .Where(im => im.object_type == "product" && im.is_primary && ids.Contains(im.object_id))
                .GroupBy(im => im.object_id)
                .Select(g => new
                {
                    ProductId = g.Key,
                    ImageId = g.OrderByDescending(x => x.id).Select(x => x.id).FirstOrDefault()
                })
                .ToListAsync();

            var imgMap = imgRows.ToDictionary(x => x.ProductId, x => (int?)x.ImageId);

            // 4) Min price map (variant active)  ✅ PriceFrom
            var priceRows = await _context.product_variants.AsNoTracking()
                .Where(v => v.is_active && ids.Contains(v.product_id))
                .GroupBy(v => v.product_id)
                .Select(g => new
                {
                    ProductId = g.Key,
                    MinPrice = g.Min(x => x.price_modifier)
                })
                .ToListAsync();

            var priceMap = priceRows.ToDictionary(x => x.ProductId, x => (decimal?)x.MinPrice);

            // 5) Build result + filter rating
            var items = new List<ProductCardVM>();

            foreach (var p in products)
            {
                var hasRating = ratingMap.TryGetValue(p.id, out var rr);
                var avg = hasRating ? rr!.Avg : 0.0;
                var cnt = hasRating ? rr!.Cnt : 0;

                if (minRating.HasValue && avg < minRating.Value) continue;

                items.Add(new ProductCardVM
                {
                    Id = p.id,
                    Sku = p.sku,
                    Name = p.name,
                    CategoryName = p.CategoryName,
                    ImageId = imgMap.TryGetValue(p.id, out var iid) ? iid : null,
                    AvgRating = Math.Round(avg, 1),
                    ReviewCount = cnt,
                    PriceFrom = priceMap.TryGetValue(p.id, out var minp) ? minp : null
                });
            }

            vm.Items = items;
            return View(vm);
        }

        // GET: /Products/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var p = await _context.products
                .Include(x => x.category)
                .Include(x => x.product_variants)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.id == id && x.is_active);

            if (p == null) return NotFound();

            // Load inventory chắc chắn từ bảng inventories
            var variantIds = p.product_variants.Where(v => v.is_active).Select(v => v.id).ToList();

            var invMap = await _context.inventories.AsNoTracking()
                .Where(x => variantIds.Contains(x.variant_id))
                .ToDictionaryAsync(x => x.variant_id, x => x);

            var vm = new ProductDetailVM
            {
                ProductId = p.id,
                ProductSku = p.sku,
                Name = p.name,
                DescriptionHtml = p.description_html,
                Material = p.material,
                CategoryName = p.category?.name ?? "Uncategorized",
                Variants = p.product_variants
                    .Where(v => v.is_active)
                    .OrderBy(v => v.sku)
                    .Select(v =>
                    {
                        invMap.TryGetValue(v.id, out var inv);
                        return new VariantVM
                        {
                            VariantId = v.id,
                            Sku = v.sku,
                            Size = v.size,
                            Color = v.color,
                            NameExtension = v.name_extension,
                            Price = v.price_modifier, // giả định
                            QtyOnHand = inv?.qty_on_hand ?? 0,
                            QtyReserved = inv?.qty_reserved ?? 0
                        };
                    })
                    .ToList()
            };

            // Reviews (approved)
            var reviews = await _context.product_reviews
                .AsNoTracking()
                .Where(r => r.product_id == id && r.is_approved)
                .ToListAsync();

            vm.ReviewCount = reviews.Count;
            var avgRating = vm.ReviewCount > 0 ? reviews.Average(r => (decimal)r.rating) : 0m;
            vm.AvgRating = Math.Round(avgRating, 1);

            // Primary image
            var img = await _context.images.AsNoTracking()
                .Where(x => x.object_type == "product" && x.object_id == id && x.is_primary)
                .OrderByDescending(x => x.id)
                .FirstOrDefaultAsync();

            ViewBag.ImageId = img?.id;

            return View(vm); // Views/Products/Details.cshtml
        }
    }
}
