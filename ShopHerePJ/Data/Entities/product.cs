using System;
using System.Collections.Generic;

namespace ShopHerePJ.Data.Entities;

public partial class product
{
    public int id { get; set; }

    public int? category_id { get; set; }

    public string sku { get; set; } = null!;

    public string name { get; set; } = null!;

    public string? description_html { get; set; }

    public string? material { get; set; }

    public bool is_active { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual category? category { get; set; }

    public virtual ICollection<product_review> product_reviews { get; set; } = new List<product_review>();

    public virtual ICollection<product_variant> product_variants { get; set; } = new List<product_variant>();
}
