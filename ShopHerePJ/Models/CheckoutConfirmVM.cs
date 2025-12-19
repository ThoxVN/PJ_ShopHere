using System.ComponentModel.DataAnnotations;
using ShopHerePJ.Data.Entities;

namespace ShopHerePJ.Models
{
    public class CheckoutConfirmVM
    {
        public List<CartLine> Items { get; set; } = new();

        public List<address> ShippingAddresses { get; set; } = new();

        [Required(ErrorMessage = "Vui lòng chọn địa chỉ giao hàng")]
        public int ShippingAddressId { get; set; }

        public string Email { get; set; } = "";
        public string? FullName { get; set; }
        public string? Phone { get; set; }

        public string PaymentMethod { get; set; } = "COD"; // chỉ COD dùng

        public string? CustomerNote { get; set; }

        public decimal Subtotal => Items.Sum(x => x.LineTotal);
        public decimal DiscountAmount { get; set; } = 0;
        public decimal ShippingFee { get; set; } = 0;
        public decimal TaxAmount { get; set; } = 0;

        public decimal GrandTotal => Subtotal - DiscountAmount + ShippingFee + TaxAmount;
        public string Currency { get; set; } = "VND";
    }
}
