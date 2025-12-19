namespace ShopHerePJ.Models
{
    public class CartLine
    {
        public int VariantId { get; set; }
        public int ProductId { get; set; }
        public int? ImageId { get; set; }
        public string ProductName { get; set; } = "";
        public string VariantSku { get; set; } = "";
        public string VariantName { get; set; } = ""; // Size/Color/Ext
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

        public int QtyAvailableSnapshot { get; set; } // để UI show nhanh (không bắt buộc)

        public decimal LineTotal => UnitPrice * Quantity;
    }
}
