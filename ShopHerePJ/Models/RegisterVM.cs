using System.ComponentModel.DataAnnotations;

namespace ShopHerePJ.Models
{
    public class RegisterVM
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, MinLength(6)]
        public string Password { get; set; } = "";

        [Required, Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; } = "";

        [Required, StringLength(100)]
        public string FullName { get; set; } = "";

        [Phone]
        public string? Phone { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
