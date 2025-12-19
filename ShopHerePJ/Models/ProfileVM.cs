using System.ComponentModel.DataAnnotations;

namespace ShopHerePJ.Models
{
    public class ProfileVM
    {
        public int UserId { get; set; }

        [EmailAddress]
        public string Email { get; set; } = "";

        [Display(Name = "Họ tên")]
        [StringLength(100)]
        public string? FullName { get; set; }

        [Display(Name = "Số điện thoại")]
        [StringLength(30)]
        public string? Phone { get; set; }

        // Optional: đổi mật khẩu (đơn giản)
        [Display(Name = "Mật khẩu hiện tại")]
        public string? CurrentPassword { get; set; }

        [Display(Name = "Mật khẩu mới")]
        [MinLength(6)]
        public string? NewPassword { get; set; }

        [Display(Name = "Xác nhận mật khẩu mới")]
        [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string? ConfirmNewPassword { get; set; }
    }
}
