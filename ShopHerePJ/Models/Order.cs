using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // <-- Thêm dòng này

namespace ShopHerePJ.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string CustomerName { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public string Address { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public ICollection<OrderItem> OrderItems { get; set; }
    }
}
