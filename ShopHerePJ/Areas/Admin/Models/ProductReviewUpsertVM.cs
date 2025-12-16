using System.ComponentModel.DataAnnotations;

namespace ShopHerePJ.Areas.Admin.Models
{
    public class ProductReviewUpsertVM
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Display(Name = "User")]
        public int? UserId { get; set; }

        [Range(1, 5)]
        [Display(Name = "Rating")]
        public byte Rating { get; set; } = 5;

        public string? Title { get; set; }
        public string? Content { get; set; }

        [Display(Name = "Approved")]
        public bool IsApproved { get; set; } = false;
    }
}
