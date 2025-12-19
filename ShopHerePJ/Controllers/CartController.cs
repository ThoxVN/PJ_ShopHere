using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopHerePJ.Data.Entities;
using ShopHerePJ.Helpers;
using ShopHerePJ.Models;

namespace ShopHerePJ.Controllers
{
    public class CartController : Controller
    {
        private const string CART_KEY = "Cart";
        private readonly ShopHereContext _context;

        public CartController(ShopHereContext context) => _context = context;

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartLine>>(CART_KEY) ?? new List<CartLine>();
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int variantId, int quantity = 1, string? returnUrl = null)
        {
            if (quantity <= 0) quantity = 1;

            var v = await _context.product_variants
                .Include(x => x.product)
                .Include(x => x.inventory)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.id == variantId && x.is_active);

            if (v == null) return NotFound(); // ✅ check trước

            // Lấy ảnh của product (ưu tiên primary nếu có)
            var imgQuery = _context.images.AsNoTracking()
                .Where(x => x.object_type == "product" && x.object_id == v.product_id);

            // nếu bạn có is_primary thì mở comment:
            // imgQuery = imgQuery.Where(x => x.is_primary);

            var img = await imgQuery
                .OrderByDescending(x => x.id)
                .FirstOrDefaultAsync();

            var imageId = img?.id;

            var avail = Math.Max(0, (v.inventory?.qty_on_hand ?? 0) - (v.inventory?.qty_reserved ?? 0));
            if (avail <= 0)
            {
                TempData["CartError"] = "Sản phẩm đã hết hàng.";
                return Redirect(returnUrl ?? Url.Action("Details", "Products", new { id = v.product_id })!);
            }
            if (quantity > avail) quantity = avail;

            var cart = HttpContext.Session.GetObjectFromJson<List<CartLine>>(CART_KEY) ?? new List<CartLine>();
            var line = cart.FirstOrDefault(x => x.VariantId == variantId);

            var variantName = string.Join(" / ", new[] {v.name_extension }
                .Where(x => !string.IsNullOrWhiteSpace(x)));

            var unitPrice = v.price_modifier;

            if (line == null)
            {
                cart.Add(new CartLine
                {
                    VariantId = v.id,
                    ProductId = v.product_id,
                    ImageId = imageId,
                    ProductName = v.product?.name ?? "Unknown",
                    VariantSku = v.sku,
                    VariantName = variantName,
                    UnitPrice = unitPrice,
                    Quantity = quantity,
                    QtyAvailableSnapshot = avail
                });
            }
            else
            {
                line.Quantity = Math.Min(line.Quantity + quantity, avail);
                line.QtyAvailableSnapshot = avail;
                line.UnitPrice = unitPrice;
                if (line.ImageId == null) line.ImageId = imageId; // ✅ optional
            }

            HttpContext.Session.SetObjectAsJson(CART_KEY, cart);
            return Redirect(returnUrl ?? Url.Action("Index")!);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int variantId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartLine>>(CART_KEY) ?? new List<CartLine>();
            cart.RemoveAll(x => x.VariantId == variantId);
            HttpContext.Session.SetObjectAsJson(CART_KEY, cart);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Clear()
        {
            HttpContext.Session.Remove(CART_KEY);
            return RedirectToAction(nameof(Index));
        }
    }
}
