using System;
using System.Collections.Generic;

namespace ShopHerePJ.Data.Entities;

public partial class product_review
{
    public int id { get; set; }

    public int product_id { get; set; }

    public int? user_id { get; set; }

    public byte rating { get; set; }

    public string? title { get; set; }

    public string? content { get; set; }

    public bool is_approved { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual product product { get; set; } = null!;

    public virtual user? user { get; set; }
}
