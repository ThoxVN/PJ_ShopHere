namespace ShopHerePJ.Models
{
    public class AdminDashboardVM
    {
        // KPI
        public int TotalOrders7d { get; set; }
        public int TotalUsersActive { get; set; }
        public int TotalUsersLocked { get; set; }
        public int TotalProductsActive { get; set; }
        public int LowStockVariants { get; set; }
        public decimal RevenueThisMonth { get; set; }

        // Chart: 7 days
        public List<string> Labels7d { get; set; } = new();
        public List<int> Orders7d { get; set; } = new();
        public List<decimal> Revenue7d { get; set; } = new();

        // Pie
        public List<string> StatusLabels { get; set; } = new();
        public List<int> StatusCounts { get; set; } = new();
    }
}
