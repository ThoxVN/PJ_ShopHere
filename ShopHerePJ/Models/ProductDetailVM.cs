using System;
using System.Collections.Generic;

namespace ShopHerePJ.Models
{
    public class ProductDetailVM
    {
        public int ProductId { get; set; }
        public string ProductSku { get; set; } = "";
        public string Name { get; set; } = "";
        public string? DescriptionHtml { get; set; }
        public string? Material { get; set; }
        public string CategoryName { get; set; } = "";
        public decimal AvgRating { get; set; }
        public int ReviewCount { get; set; }


        public List<VariantVM> Variants { get; set; } = new();
    }

    public class VariantVM
    {
        public int VariantId { get; set; }
        public string Sku { get; set; } = "";
        public string Size { get; set; } = "";
        public string Color { get; set; } = "";
        public string? NameExtension { get; set; }

        // NOTE: hiện entity không có base price; mình tạm coi price_modifier là “giá”
        public decimal Price { get; set; }

        public int QtyOnHand { get; set; }
        public int QtyReserved { get; set; }
        public int QtyAvailable => Math.Max(0, QtyOnHand - QtyReserved);

        public string DisplayName =>
            string.Join(" / ", new[] { Size, Color, NameExtension }.Where(x => !string.IsNullOrWhiteSpace(x)));
    }
}
