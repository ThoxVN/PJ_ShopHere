using System.ComponentModel.DataAnnotations;  // <-- Thêm dòng này
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopHerePJ.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        [ForeignKey("Order")]
        public int OrderId { get; set; }
        public Order Order { get; set; }

        [Required]
        public int ProductId { get; set; } // id sản phẩm

        [Required]
        public string ProductName { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal Price { get; set; }
    }
}
