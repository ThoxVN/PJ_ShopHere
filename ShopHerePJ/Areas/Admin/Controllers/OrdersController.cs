using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopHerePJ.Areas.Admin.Models;
using ShopHerePJ.Data.Entities;

namespace ShopHerePJ.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrdersController : Controller
    {
        private readonly ShopHereContext _context;
        public OrdersController(ShopHereContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            var data = await _context.orders
                .Include(o => o.user)
                .AsNoTracking()
                .OrderByDescending(o => o.id)
                .ToListAsync();

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search(string search)
        {
            ViewBag.Search = search;

            IQueryable<order> q = _context.orders
                .Include(o => o.user)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                q = q.Where(o =>
                    o.order_number.Contains(search) ||
                    o.status.Contains(search) ||
                    o.payment_status.Contains(search) ||
                    (o.tracking_number != null && o.tracking_number.Contains(search)) ||
                    (o.user != null && (o.user.email.Contains(search) || (o.user.full_name != null && o.user.full_name.Contains(search))))
                );
            }

            var data = await q.OrderByDescending(o => o.id).ToListAsync();
            return View("Index", data);
        }

        public async Task<IActionResult> Create()
        {
            await LoadDropdowns(null);
            return View(new OrderUpsertVM
            {
                OrderNumber = $"ORD-{DateTime.Now:yyyyMMddHHmmss}",
                Status = "New",
                PaymentStatus = "Unpaid",
                Currency = "VND"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderUpsertVM vm)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdowns(vm.UserId);
                return View(vm);
            }

            var entity = new order
            {
                order_number = vm.OrderNumber.Trim(),
                user_id = vm.UserId,
                status = vm.Status,
                payment_status = vm.PaymentStatus,
                payment_method = vm.PaymentMethod,
                paid_at = vm.PaidAt,
                shipping_method = vm.ShippingMethod,
                tracking_number = vm.TrackingNumber,
                shipped_at = vm.ShippedAt,
                delivered_at = vm.DeliveredAt,
                shipping_address_id = vm.ShippingAddressId,
                billing_address_id = vm.BillingAddressId,

                subtotal_amount = vm.SubtotalAmount,
                discount_amount = vm.DiscountAmount,
                shipping_fee = vm.ShippingFee,
                tax_amount = vm.TaxAmount,
                grand_total = vm.GrandTotal,

                currency = vm.Currency,
                customer_note = vm.CustomerNote,
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            _context.orders.Add(entity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Edit), new { id = entity.id });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _context.orders.FirstOrDefaultAsync(o => o.id == id);
            if (entity == null) return NotFound();

            await LoadDropdowns(entity.user_id);

            var vm = new OrderUpsertVM
            {
                Id = entity.id,
                OrderNumber = entity.order_number,
                UserId = entity.user_id,
                Status = entity.status,
                PaymentStatus = entity.payment_status,
                PaymentMethod = entity.payment_method,
                PaidAt = entity.paid_at,
                ShippingMethod = entity.shipping_method,
                TrackingNumber = entity.tracking_number,
                ShippedAt = entity.shipped_at,
                DeliveredAt = entity.delivered_at,
                ShippingAddressId = entity.shipping_address_id,
                BillingAddressId = entity.billing_address_id,
                SubtotalAmount = entity.subtotal_amount,
                DiscountAmount = entity.discount_amount,
                ShippingFee = entity.shipping_fee,
                TaxAmount = entity.tax_amount,
                GrandTotal = entity.grand_total,
                Currency = entity.currency,
                CustomerNote = entity.customer_note
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OrderUpsertVM vm)
        {
            if (id != vm.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                await LoadDropdowns(vm.UserId);
                return View(vm);
            }

            var entity = await _context.orders.FirstOrDefaultAsync(o => o.id == id);
            if (entity == null) return NotFound();

            entity.order_number = vm.OrderNumber.Trim();
            entity.user_id = vm.UserId;

            entity.status = vm.Status;
            entity.payment_status = vm.PaymentStatus;
            entity.payment_method = vm.PaymentMethod;
            entity.paid_at = vm.PaidAt;

            entity.shipping_method = vm.ShippingMethod;
            entity.tracking_number = vm.TrackingNumber;
            entity.shipped_at = vm.ShippedAt;
            entity.delivered_at = vm.DeliveredAt;

            entity.shipping_address_id = vm.ShippingAddressId;
            entity.billing_address_id = vm.BillingAddressId;

            // totals (thường sẽ do OrderItems recalc, nhưng vẫn cho sửa)
            entity.subtotal_amount = vm.SubtotalAmount;
            entity.discount_amount = vm.DiscountAmount;
            entity.shipping_fee = vm.ShippingFee;
            entity.tax_amount = vm.TaxAmount;
            entity.grand_total = vm.GrandTotal;

            entity.currency = vm.Currency;
            entity.customer_note = vm.CustomerNote;

            entity.updated_at = DateTime.Now;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Edit), new { id });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.orders
                .Include(o => o.user)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.id == id);

            if (entity == null) return NotFound();
            return View(entity);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entity = await _context.orders.FirstOrDefaultAsync(o => o.id == id);
            if (entity == null) return NotFound();

            _context.orders.Remove(entity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDropdowns(int? selectedUserId)
        {
            var users = await _context.users.AsNoTracking()
                .OrderBy(u => u.email)
                .Select(u => new { u.userid, Label = u.email + (u.full_name != null ? " - " + u.full_name : "") })
                .ToListAsync();
            ViewBag.UserSelect = new SelectList(users, "userid", "Label", selectedUserId);

            var addresses = await _context.addresses.AsNoTracking()
                .OrderByDescending(a => a.updated_at)
                .Select(a => new
                {
                    a.id,
                    Label = (a.user_id.HasValue ? ("User#" + a.user_id + " | ") : "")
                            + (a.recipient_name ?? "N/A") + " | "
                            + (a.street ?? "") + " " + (a.ward ?? "") + " " + (a.district ?? "") + " " + (a.city ?? "")
                })
                .ToListAsync();

            ViewBag.AddressSelect = new SelectList(addresses, "id", "Label");
        }
    }
}
