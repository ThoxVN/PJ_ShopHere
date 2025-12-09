using System;
using System.Collections.Generic;

namespace ShopHerePJ.Data.Entities;

public partial class cart_item
{
    public int id { get; set; }

    public int cart_id { get; set; }

    public int variant_id { get; set; }

    public int quantity { get; set; }

    public decimal unit_price { get; set; }

    public decimal line_discount_amount { get; set; }

    public decimal line_total { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual cart cart { get; set; } = null!;

    public virtual product_variant variant { get; set; } = null!;
}
