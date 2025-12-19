using System.ComponentModel.DataAnnotations;

namespace ShopHerePJ.Models
{
    public class LoginVM
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";

        public bool RememberMe { get; set; } = true;

        public string? ReturnUrl { get; set; }
    }
}
