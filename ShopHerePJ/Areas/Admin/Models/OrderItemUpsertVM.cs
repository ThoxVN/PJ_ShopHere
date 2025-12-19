using System.ComponentModel.DataAnnotations;

namespace ShopHerePJ.Areas.Admin.Models
{
    public class OrderItemUpsertVM
    {
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        [Display(Name = "Variant")]
        public int VariantId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;

        public decimal UnitPrice { get; set; }
        public decimal LineDiscountAmount { get; set; }
    }
}
