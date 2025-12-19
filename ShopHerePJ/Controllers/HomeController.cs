using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopHerePJ.Data.Entities;
using ShopHerePJ.Models;

namespace ShopHerePJ.Controllers
{
    public class HomeController : Controller
    {
        private readonly ShopHereContext _context;
        public HomeController(ShopHereContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            // lấy 8 sản phẩm mới nhất (active)
            var products = await _context.products
                .Include(p => p.category)
                .AsNoTracking()
                .Where(p => p.is_active)
                .OrderByDescending(p => p.created_at)
                .Take(8)
                .ToListAsync();

            var ids = products.Select(p => p.id).ToList();

            var imgMap = await _context.images.AsNoTracking()
                .Where(i => i.object_type == "product" && ids.Contains(i.object_id) && i.is_primary)
                .GroupBy(i => i.object_id)
                .Select(g => new { ProductId = g.Key, ImageId = g.OrderByDescending(x => x.id).First().id })
                .ToDictionaryAsync(x => x.ProductId, x => x.ImageId);

            var ratingMap = await _context.product_reviews.AsNoTracking()
                .Where(r => ids.Contains(r.product_id) && r.is_approved)
                .GroupBy(r => r.product_id)
                .Select(g => new {
                    ProductId = g.Key,
                    Avg = g.Average(x => (double)x.rating),
                    Cnt = g.Count()
                })
                .ToDictionaryAsync(x => x.ProductId, x => (x.Avg, x.Cnt));

            var vm = new StoreHomeViewModel
            {
                NewArrivals = products.Select(p =>
                {
                    var catKey = (p.category?.slug ?? p.category?.name ?? "all").ToLower().Replace(" ", "-");
                    var imgId = imgMap.ContainsKey(p.id) ? imgMap[p.id] : (int?)null;
                    var rating = ratingMap.ContainsKey(p.id) ? ratingMap[p.id] : (0d, 0);

                    return new ProductCardVM
                    {
                        Id = p.id,
                        Name = p.name,
                        Sku = p.sku,
                        CategoryName = p.category?.name ?? "Uncategorized",
                        CategoryKey = catKey,
                        ImageId = imgId,
                        AvgRating = rating.Item1,
                        ReviewCount = rating.Item2,
                        PriceFrom = null // nếu có bảng variant/price thì bạn map vào đây
                    };
                }).ToList()
            };

            return View(vm);
        }
    }
}
