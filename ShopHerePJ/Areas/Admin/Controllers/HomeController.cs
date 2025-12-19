using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopHerePJ.Data.Entities;
using ShopHerePJ.Models;

namespace ShopHerePJ.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class HomeController : Controller
    {
        private readonly ShopHereContext _context;
        public HomeController(ShopHereContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            // Bạn đang seed bằng SYSUTCDATETIME() nhưng code có lúc DateTime.Now.
            // Làm đơn giản: dùng DateTime.Today theo server.
            var today = DateTime.Today;
            var start7d = today.AddDays(-6);
            var endExclusive = today.AddDays(1);

            // ========= KPI =========
            var totalUsersActive = await _context.users.AsNoTracking()
                .CountAsync(u => u.status == "active");

            var totalUsersLocked = await _context.users.AsNoTracking()
                .CountAsync(u => u.status == "locked");

            var totalProductsActive = await _context.products.AsNoTracking()
                .CountAsync(p => p.is_active);

            // Low stock theo variant (avail <= 5)
            var lowStockVariants = await _context.inventories.AsNoTracking()
                .CountAsync(inv => (inv.qty_on_hand - inv.qty_reserved) <= 5);

            // Orders 7 ngày
            var totalOrders7d = await _context.orders.AsNoTracking()
                .CountAsync(o => o.created_at >= start7d && o.created_at < endExclusive);

            // Revenue tháng này (loại cancelled/refunded cho “hợp lý”)
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var nextMonth = monthStart.AddMonths(1);

            var revenueThisMonth = await _context.orders.AsNoTracking()
                .Where(o => o.created_at >= monthStart && o.created_at < nextMonth)
                .Where(o => o.status != "cancelled" && o.status != "refunded")
                .SumAsync(o => (decimal?)o.grand_total) ?? 0m;

            // ========= Chart: Orders + Revenue 7 ngày =========
            var rows7d = await _context.orders.AsNoTracking()
                .Where(o => o.created_at >= start7d && o.created_at < endExclusive)
                .GroupBy(o => o.created_at.Date)
                .Select(g => new
                {
                    Day = g.Key,
                    Count = g.Count(),
                    Revenue = g.Sum(x => x.grand_total)
                })
                .ToListAsync();

            var map7d = rows7d.ToDictionary(x => x.Day, x => x);

            var labels = new List<string>();
            var orders = new List<int>();
            var revenue = new List<decimal>();

            for (int i = 0; i < 7; i++)
            {
                var d = start7d.AddDays(i).Date;
                labels.Add(d.ToString("dd/MM"));
                if (map7d.TryGetValue(d, out var r))
                {
                    orders.Add(r.Count);
                    revenue.Add(r.Revenue);
                }
                else
                {
                    orders.Add(0);
                    revenue.Add(0);
                }
            }

            // ========= Pie: status breakdown 7 ngày =========
            var statusRows = await _context.orders.AsNoTracking()
                .Where(o => o.created_at >= start7d && o.created_at < endExclusive)
                .GroupBy(o => o.status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            // order hiển thị cố định cho đẹp
            var statusOrder = new[]
            {
                "pending_payment","paid","processing","shipped","completed","cancelled","refunded"
            };

            var statusDict = statusRows.ToDictionary(x => x.Status, x => x.Count);

            var statusLabels = new List<string>();
            var statusCounts = new List<int>();

            foreach (var s in statusOrder)
            {
                if (!statusDict.TryGetValue(s, out var c)) c = 0;
                // label thân thiện
                statusLabels.Add(s switch
                {
                    "pending_payment" => "Pending",
                    "paid" => "Paid",
                    "processing" => "Processing",
                    "shipped" => "Shipped",
                    "completed" => "Completed",
                    "cancelled" => "Cancelled",
                    "refunded" => "Refunded",
                    _ => s
                });
                statusCounts.Add(c);
            }

            var vm = new AdminDashboardVM
            {
                TotalOrders7d = totalOrders7d,
                TotalUsersActive = totalUsersActive,
                TotalUsersLocked = totalUsersLocked,
                TotalProductsActive = totalProductsActive,
                LowStockVariants = lowStockVariants,
                RevenueThisMonth = revenueThisMonth,

                Labels7d = labels,
                Orders7d = orders,
                Revenue7d = revenue,

                StatusLabels = statusLabels,
                StatusCounts = statusCounts
            };

            return View(vm);
        }
    }
}
