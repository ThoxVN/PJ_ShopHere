namespace ShopHerePJ.Models
{
    public class StoreHomeViewModel
    {
        public List<string> Tabs { get; set; } = new() { "All", "Women", "Men", "Shoes", "Bags", "Accessories" };
        public List<ProductCardVM> NewArrivals { get; set; } = new();
    }

    public class ProductCardVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Sku { get; set; } = "";
        public string CategoryName { get; set; } = "Uncategorized";
        public string CategoryKey { get; set; } = "all"; // để filter tab
        public int? ImageId { get; set; }
        public double AvgRating { get; set; }
        public int ReviewCount { get; set; }
        public decimal? PriceFrom { get; set; } // nếu chưa có giá thì để null
    }
}
