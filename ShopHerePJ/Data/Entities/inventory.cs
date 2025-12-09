using System;
using System.Collections.Generic;

namespace ShopHerePJ.Data.Entities;

public partial class inventory
{
    public int id { get; set; }

    public int variant_id { get; set; }

    public int qty_on_hand { get; set; }

    public int qty_reserved { get; set; }

    public DateTime updated_at { get; set; }

    public virtual product_variant variant { get; set; } = null!;
}
