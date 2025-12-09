using System.Collections.Generic;
using ShopHerePJ.Data.Entities;

namespace ShopHerePJ.Models
{
    public class StoreIndexViewModel
    {
        public IEnumerable<product> Products { get; set; } = new List<product>();
        public IEnumerable<category> Categories { get; set; } = new List<category>();
        public int? SelectedCategoryId { get; set; }
    }
}
