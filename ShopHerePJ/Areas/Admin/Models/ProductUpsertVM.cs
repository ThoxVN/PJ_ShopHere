using System.ComponentModel.DataAnnotations;

namespace ShopHerePJ.Areas.Admin.Models
{
    public class ProductUpsertVM
    {
        public int Id { get; set; }

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        [Required]
        [Display(Name = "SKU")]
        public string Sku { get; set; } = null!;

        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; } = null!;

        [Display(Name = "Description (HTML)")]
        public string? DescriptionHtml { get; set; }

        [Display(Name = "Material")]
        public string? Material { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
