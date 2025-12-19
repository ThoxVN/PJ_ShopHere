using System.ComponentModel.DataAnnotations;

namespace ShopHerePJ.Models
{
    public class AccAddressUpsert
    {
        public int Id { get; set; }

        [Required]
        public string Type { get; set; } = "Shipping"; // mặc định Shipping

        [Display(Name = "Người nhận")]
        [StringLength(100)]
        public string? RecipientName { get; set; }

        [Display(Name = "Số điện thoại")]
        [StringLength(30)]
        public string? Phone { get; set; }

        [Display(Name = "Địa chỉ")]
        [StringLength(255)]
        public string? Street { get; set; }

        [Display(Name = "Phường/Xã")]
        [StringLength(100)]
        public string? Ward { get; set; }

        [Display(Name = "Quận/Huyện")]
        [StringLength(100)]
        public string? District { get; set; }

        [Display(Name = "Tỉnh/Thành phố")]
        [StringLength(100)]
        public string? City { get; set; }

        [Display(Name = "Postal code")]
        [StringLength(20)]
        public string? PostalCode { get; set; }

        [Required]
        [Display(Name = "Quốc gia")]
        [StringLength(80)]
        public string Country { get; set; } = "Vietnam";

        [Display(Name = "Đặt làm mặc định")]
        public bool IsDefault { get; set; }
    }
}
