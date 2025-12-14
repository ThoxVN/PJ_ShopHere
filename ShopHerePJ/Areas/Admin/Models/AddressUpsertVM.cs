using System.ComponentModel.DataAnnotations;

namespace ShopHerePJ.Areas.Admin.Models
{
    public class AddressUpsertVM
    {
        public int Id { get; set; }

        [Display(Name = "User")]
        public int? UserId { get; set; }

        [Required]
        [Display(Name = "Type")]
        public string Type { get; set; } = "Shipping"; // Shipping/Billing...

        [Display(Name = "Recipient name")]
        public string? RecipientName { get; set; }

        [Display(Name = "Phone")]
        public string? Phone { get; set; }

        public string? Street { get; set; }
        public string? Ward { get; set; }
        public string? District { get; set; }
        public string? City { get; set; }

        [Display(Name = "Postal code")]
        public string? PostalCode { get; set; }

        [Required]
        public string Country { get; set; } = "Vietnam";

        [Display(Name = "Default")]
        public bool IsDefault { get; set; }
    }
}
