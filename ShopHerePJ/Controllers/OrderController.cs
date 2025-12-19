using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopHerePJ.Data.Entities;
using ShopHerePJ.Helpers;
using ShopHerePJ.Models;
using System.Security.Claims;

namespace ShopHerePJ.Controllers
{
    public class OrderController : Controller
    {
        private const string CART_KEY = "Cart";
        private const string CHECKOUT_SELECTED_KEY = "CheckoutSelected";

        private readonly ShopHereContext _context;
        private readonly ILogger<OrderController> _logger;

        public OrderController(ShopHereContext context, ILogger<OrderController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private int CurrentUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(idStr!);
        }

        // POST: từ Cart -> lưu selectedVariantIds vào session
        // Nếu chưa login thì redirect Login nhưng vẫn giữ selection
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(int[] selectedVariantIds)
        {
            if (selectedVariantIds == null || selectedVariantIds.Length == 0)
            {
                TempData["CartError"] = "Vui lòng chọn ít nhất 1 sản phẩm để thanh toán.";
                return RedirectToAction("Index", "Cart");
            }

            HttpContext.Session.SetObjectAsJson(CHECKOUT_SELECTED_KEY, selectedVariantIds.Distinct().ToList());

            if (!(User?.Identity?.IsAuthenticated ?? false))
            {
                // login xong quay lại trang checkout confirm
                return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Checkout", "Order") });
            }

            return RedirectToAction(nameof(Checkout)); // GET
        }

        // GET: /Order/Checkout (xác nhận thông tin + địa chỉ + payment)
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var uid = CurrentUserId();

            var cart = HttpContext.Session.GetObjectFromJson<List<CartLine>>(CART_KEY) ?? new();
            var selected = HttpContext.Session.GetObjectFromJson<List<int>>(CHECKOUT_SELECTED_KEY) ?? new();

            var items = cart.Where(x => selected.Contains(x.VariantId)).ToList();
            if (!items.Any())
            {
                TempData["CartError"] = "Không có sản phẩm nào được chọn để thanh toán.";
                return RedirectToAction("Index", "Cart");
            }

            var u = await _context.users.AsNoTracking().FirstOrDefaultAsync(x => x.userid == uid);
            if (u == null) return RedirectToAction("Logout", "Auth");

            var addrs = await _context.addresses.AsNoTracking()
                .Where(a => a.user_id == uid && a.type == "Shipping")
                .OrderByDescending(a => a.is_default)
                .ThenByDescending(a => a.updated_at)
                .ToListAsync();

            if (!addrs.Any())
            {
                TempData["CartError"] = "Bạn chưa có địa chỉ giao hàng. Vui lòng thêm địa chỉ trước khi checkout.";
                return RedirectToAction("AddressCreate", "Account");
            }

            var defaultAddr = addrs.FirstOrDefault(a => a.is_default) ?? addrs.First();

            var vm = new CheckoutConfirmVM
            {
                Items = items,
                ShippingAddresses = addrs,
                ShippingAddressId = defaultAddr.id,
                Email = u.email,
                FullName = u.full_name,
                Phone = u.phone,
                PaymentMethod = "COD",
                Currency = "VND"
            };

            return View(vm); // Views/Order/CheckoutConfirm.cshtml
        }

        // POST: /Order/PlaceOrder -> tạo order + items
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutConfirmVM vm)
        {
            var uid = CurrentUserId();

            // enforce COD only
            vm.PaymentMethod = "COD";

            var cart = HttpContext.Session.GetObjectFromJson<List<CartLine>>(CART_KEY) ?? new();
            var selected = HttpContext.Session.GetObjectFromJson<List<int>>(CHECKOUT_SELECTED_KEY) ?? new();

            var selectedLines = cart.Where(x => selected.Contains(x.VariantId)).ToList();
            if (!selectedLines.Any())
            {
                TempData["CartError"] = "Không có sản phẩm nào được chọn để tạo đơn.";
                return RedirectToAction("Index", "Cart");
            }

            // Validate address
            var shipAddr = await _context.addresses.AsNoTracking()
                .FirstOrDefaultAsync(a => a.id == vm.ShippingAddressId && a.user_id == uid && a.type == "Shipping");

            if (shipAddr == null)
            {
                TempData["CartError"] = "Địa chỉ giao hàng không hợp lệ.";
                return RedirectToAction(nameof(Checkout));
            }

            var variantIds = selectedLines.Select(x => x.VariantId).Distinct().ToList();

            // Load variants + product (để snapshot)
            var variants = await _context.product_variants
                .Include(v => v.product)
                .Where(v => variantIds.Contains(v.id) && v.is_active)
                .ToListAsync();

            if (variants.Count != variantIds.Count)
            {
                TempData["CartError"] = "Một số sản phẩm không còn khả dụng. Vui lòng kiểm tra lại giỏ hàng.";
                return RedirectToAction("Index", "Cart");
            }

            // Load inventory chắc chắn từ bảng inventories (không phụ thuộc navigation)
            var invList = await _context.inventories
                .Where(x => variantIds.Contains(x.variant_id))
                .ToListAsync();

            var invMap = invList.ToDictionary(x => x.variant_id, x => x);

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                // Check tồn kho
                foreach (var line in selectedLines)
                {
                    if (!invMap.TryGetValue(line.VariantId, out var inv))
                    {
                        TempData["CartError"] = "Sản phẩm chưa có tồn kho. Vui lòng cập nhật Inventory trước.";
                        await tx.RollbackAsync();
                        return RedirectToAction("Index", "Cart");
                    }

                    var avail = Math.Max(0, inv.qty_on_hand - inv.qty_reserved);
                    if (avail < line.Quantity)
                    {
                        var v = variants.First(x => x.id == line.VariantId);
                        TempData["CartError"] = $"Sản phẩm '{v.product?.name}' ({v.sku}) không đủ tồn. Còn {avail}.";
                        await tx.RollbackAsync();
                        return RedirectToAction("Index", "Cart");
                    }
                }

                var now = DateTime.Now;
                var orderNo = $"ORD-{now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
                var subtotal = selectedLines.Sum(x => x.UnitPrice * x.Quantity);

                var order = new order
                {
                    order_number = orderNo,
                    user_id = uid,
                    cart_id = null,

                    status = "pending_payment",
                    payment_status = "unpaid",
                    payment_method = "cod",
                    paid_at = null,

                    shipping_method = "standard",
                    tracking_number = null,
                    shipped_at = null,
                    delivered_at = null,

                    shipping_address_id = shipAddr.id,
                    billing_address_id = shipAddr.id,

                    subtotal_amount = subtotal,
                    discount_amount = 0,
                    shipping_fee = 0,
                    tax_amount = 0,
                    grand_total = subtotal,
                    currency = "VND",

                    customer_note = vm.CustomerNote,

                    created_at = now,
                    updated_at = now
                };


                _context.orders.Add(order);
                await _context.SaveChangesAsync(); // lấy order.id

                // Create order items + reserve inventory
                foreach (var line in selectedLines)
                {
                    var v = variants.First(x => x.id == line.VariantId);

                    var variantNameSnapshot = string.Join(" / ",
                        new[] { v.size, v.color, v.name_extension }
                        .Where(x => !string.IsNullOrWhiteSpace(x)));

                    var unitPrice = line.UnitPrice;
                    var lineDiscount = 0m;
                    var lineTotal = unitPrice * line.Quantity - lineDiscount;

                    _context.order_items.Add(new order_item
                    {
                        order_id = order.id,
                        variant_id = v.id,
                        product_name_snapshot = v.product?.name ?? "Unknown",
                        sku_snapshot = v.sku,
                        variant_name_snapshot = variantNameSnapshot,

                        quantity = line.Quantity,
                        unit_price = unitPrice,
                        line_discount_amount = lineDiscount,
                        line_total = lineTotal,

                        created_at = now,
                        updated_at = now
                    });

                    // Reserve tồn kho (inv chắc chắn có)
                    var inv = invMap[v.id];
                    inv.qty_reserved += line.Quantity;
                    inv.updated_at = now;
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                // Remove ONLY selected khỏi cart
                cart.RemoveAll(x => selected.Contains(x.VariantId));
                HttpContext.Session.SetObjectAsJson(CART_KEY, cart);
                HttpContext.Session.Remove(CHECKOUT_SELECTED_KEY);

                return RedirectToAction("OrderSuccess", "Order", new { id = order.id });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();

                _logger.LogError(ex, "PlaceOrder failed for userId={UserId}", uid);

                // show lỗi thật (để debug)
                var msg = ex.InnerException?.Message ?? ex.Message;
                TempData["CartError"] = "Tạo đơn thất bại: " + msg;

                return RedirectToAction(nameof(Checkout));
            }
        }

        // GET: /Order/OrderSuccess/12
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> OrderSuccess(int id)
        {
            var uid = CurrentUserId();

            var o = await _context.orders
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.id == id && x.user_id == uid);

            if (o == null) return NotFound();

            return View(o); // Views/Order/OrderSuccess.cshtml
        }

    }
}
