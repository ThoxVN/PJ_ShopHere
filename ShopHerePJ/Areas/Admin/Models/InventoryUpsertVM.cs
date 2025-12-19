using System.ComponentModel.DataAnnotations;

namespace ShopHerePJ.Areas.Admin.Models
{
    public class InventoryUpsertVM
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Variant")]
        public int VariantId { get; set; }

        [Display(Name = "Qty On Hand")]
        public int QtyOnHand { get; set; }

        [Display(Name = "Qty Reserved")]
        public int QtyReserved { get; set; }
    }
}
