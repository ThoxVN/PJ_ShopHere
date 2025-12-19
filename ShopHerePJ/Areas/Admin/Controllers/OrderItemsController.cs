using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopHerePJ.Areas.Admin.Models;
using ShopHerePJ.Data.Entities;

namespace ShopHerePJ.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderItemsController : Controller
    {
        private readonly ShopHereContext _context;
        public OrderItemsController(ShopHereContext context) => _context = context;

        public async Task<IActionResult> Index(int orderId)
        {
            var order = await _context.orders.AsNoTracking().FirstOrDefaultAsync(o => o.id == orderId);
            if (order == null) return NotFound();

            ViewBag.OrderId = orderId;
            ViewBag.OrderNumber = order.order_number;

            var data = await _context.order_items
                .Include(i => i.variant).ThenInclude(v => v.product)
                .AsNoTracking()
                .Where(i => i.order_id == orderId)
                .OrderBy(i => i.id)
                .ToListAsync();

            return View(data);
        }

        public IActionResult ByOrder(int orderId) => RedirectToAction(nameof(Index), new { orderId });

        public async Task<IActionResult> Create(int orderId)
        {
            await LoadVariantDropdown();
            ViewBag.OrderId = orderId;

            return View(new OrderItemUpsertVM { OrderId = orderId, Quantity = 1 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderItemUpsertVM vm)
        {
            if (!ModelState.IsValid)
            {
                await LoadVariantDropdown();
                ViewBag.OrderId = vm.OrderId;
                return View(vm);
            }

            var variant = await _context.product_variants
                .Include(v => v.product)
                .FirstOrDefaultAsync(v => v.id == vm.VariantId);
            if (variant == null) return BadRequest("Variant not found.");

            // Snapshot
            var productName = variant.product?.name ?? "Unknown";
            var sku = variant.sku;
            var variantName = string.Join(" / ", new[] { variant.size, variant.color, variant.name_extension }.Where(x => !string.IsNullOrWhiteSpace(x)));

            var lineTotal = (vm.UnitPrice * vm.Quantity) - vm.LineDiscountAmount;

            var entity = new order_item
            {
                order_id = vm.OrderId,
                variant_id = vm.VariantId,
                product_name_snapshot = productName,
                sku_snapshot = sku,
                variant_name_snapshot = variantName,

                quantity = vm.Quantity,
                unit_price = vm.UnitPrice,
                line_discount_amount = vm.LineDiscountAmount,
                line_total = lineTotal,

                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            _context.order_items.Add(entity);
            await _context.SaveChangesAsync();

            await RecalcOrderTotals(vm.OrderId);

            return RedirectToAction(nameof(Index), new { orderId = vm.OrderId });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _context.order_items
                .Include(i => i.variant).ThenInclude(v => v.product)
                .FirstOrDefaultAsync(i => i.id == id);

            if (entity == null) return NotFound();

            await LoadVariantDropdown(entity.variant_id);
            ViewBag.OrderId = entity.order_id;

            var vm = new OrderItemUpsertVM
            {
                Id = entity.id,
                OrderId = entity.order_id,
                VariantId = entity.variant_id,
                Quantity = entity.quantity,
                UnitPrice = entity.unit_price,
                LineDiscountAmount = entity.line_discount_amount
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OrderItemUpsertVM vm)
        {
            if (id != vm.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                await LoadVariantDropdown(vm.VariantId);
                ViewBag.OrderId = vm.OrderId;
                return View(vm);
            }

            var entity = await _context.order_items.FirstOrDefaultAsync(i => i.id == id);
            if (entity == null) return NotFound();

            var variant = await _context.product_variants
                .Include(v => v.product)
                .FirstOrDefaultAsync(v => v.id == vm.VariantId);
            if (variant == null) return BadRequest("Variant not found.");

            // Update snapshot
            entity.variant_id = vm.VariantId;
            entity.product_name_snapshot = variant.product?.name ?? "Unknown";
            entity.sku_snapshot = variant.sku;
            entity.variant_name_snapshot = string.Join(" / ", new[] { variant.size, variant.color, variant.name_extension }.Where(x => !string.IsNullOrWhiteSpace(x)));

            entity.quantity = vm.Quantity;
            entity.unit_price = vm.UnitPrice;
            entity.line_discount_amount = vm.LineDiscountAmount;
            entity.line_total = (vm.UnitPrice * vm.Quantity) - vm.LineDiscountAmount;

            entity.updated_at = DateTime.Now;

            await _context.SaveChangesAsync();
            await RecalcOrderTotals(entity.order_id);

            return RedirectToAction(nameof(Index), new { orderId = entity.order_id });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.order_items
                .Include(i => i.variant).ThenInclude(v => v.product)
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.id == id);

            if (entity == null) return NotFound();

            ViewBag.OrderId = entity.order_id;
            return View(entity);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entity = await _context.order_items.FirstOrDefaultAsync(i => i.id == id);
            if (entity == null) return NotFound();

            var orderId = entity.order_id;

            _context.order_items.Remove(entity);
            await _context.SaveChangesAsync();

            await RecalcOrderTotals(orderId);

            return RedirectToAction(nameof(Index), new { orderId });
        }

        private async Task LoadVariantDropdown(int? selectedVariantId = null)
        {
            var variants = await _context.product_variants
                .Include(v => v.product)
                .AsNoTracking()
                .OrderBy(v => v.product.name)
                .ThenBy(v => v.sku)
                .Select(v => new
                {
                    v.id,
                    Label = v.product.name + " | " + v.sku + " | " + v.size + " / " + v.color
                })
                .ToListAsync();

            ViewBag.VariantSelect = new SelectList(variants, "id", "Label", selectedVariantId);
        }

        private async Task RecalcOrderTotals(int orderId)
        {
            var order = await _context.orders.FirstOrDefaultAsync(o => o.id == orderId);
            if (order == null) return;

            var items = await _context.order_items.AsNoTracking()
                .Where(i => i.order_id == orderId)
                .ToListAsync();

            var subtotal = items.Sum(i => i.unit_price * i.quantity);
            var discount = items.Sum(i => i.line_discount_amount);

            order.subtotal_amount = subtotal;
            order.discount_amount = discount;

            // giữ shipping_fee và tax_amount như admin nhập
            order.grand_total = subtotal - discount + order.shipping_fee + order.tax_amount;
            order.updated_at = DateTime.Now;

            await _context.SaveChangesAsync();
        }
    }
}
