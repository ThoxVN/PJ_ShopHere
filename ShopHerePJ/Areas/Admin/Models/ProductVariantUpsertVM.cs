using System.ComponentModel.DataAnnotations;

namespace ShopHerePJ.Areas.Admin.Models
{
    public class ProductVariantUpsertVM
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Required]
        [Display(Name = "SKU")]
        public string Sku { get; set; } = null!;

        [Display(Name = "Name Extension")]
        public string? NameExtension { get; set; }

        [Required]
        public string Size { get; set; } = null!;

        [Required]
        public string Color { get; set; } = null!;

        [Display(Name = "Price Modifier")]
        public decimal PriceModifier { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
