using System.ComponentModel.DataAnnotations;

namespace ShopHerePJ.Areas.Admin.Models
{
    public class UserUpsertVM
    {
        public int UserId { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Display(Name = "Full name")]
        public string? FullName { get; set; }

        public string? Phone { get; set; }

        [Required]
        public string Role { get; set; } = "Customer";

        [Required]
        public string Status { get; set; } = "Active";

        // Create: bắt buộc; Edit: optional
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Confirm password không khớp.")]
        public string? ConfirmPassword { get; set; }
    }
}
