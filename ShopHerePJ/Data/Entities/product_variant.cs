using System;
using System.Collections.Generic;

namespace ShopHerePJ.Data.Entities;

public partial class product_variant
{
    public int id { get; set; }

    public int product_id { get; set; }

    public string sku { get; set; } = null!;

    public string? name_extension { get; set; }

    public string size { get; set; } = null!;

    public string color { get; set; } = null!;

    public decimal price_modifier { get; set; }

    public bool is_active { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual ICollection<cart_item> cart_items { get; set; } = new List<cart_item>();

    public virtual inventory? inventory { get; set; }

    public virtual ICollection<order_item> order_items { get; set; } = new List<order_item>();

    public virtual product product { get; set; } = null!;
}
