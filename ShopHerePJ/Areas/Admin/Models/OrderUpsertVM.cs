using System.ComponentModel.DataAnnotations;

namespace ShopHerePJ.Areas.Admin.Models
{
    public class OrderUpsertVM
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Order Number")]
        public string OrderNumber { get; set; } = null!;

        [Display(Name = "User")]
        public int? UserId { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = "New";

        [Display(Name = "Payment Status")]
        public string PaymentStatus { get; set; } = "Unpaid";

        [Display(Name = "Payment Method")]
        public string? PaymentMethod { get; set; }

        [Display(Name = "Paid At")]
        public DateTime? PaidAt { get; set; }

        [Display(Name = "Shipping Method")]
        public string? ShippingMethod { get; set; }

        [Display(Name = "Tracking Number")]
        public string? TrackingNumber { get; set; }

        [Display(Name = "Shipped At")]
        public DateTime? ShippedAt { get; set; }

        [Display(Name = "Delivered At")]
        public DateTime? DeliveredAt { get; set; }

        [Display(Name = "Shipping Address")]
        public int? ShippingAddressId { get; set; }

        [Display(Name = "Billing Address")]
        public int? BillingAddressId { get; set; }

        public decimal SubtotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }

        [Required]
        public string Currency { get; set; } = "VND";

        public string? CustomerNote { get; set; }
    }
}
