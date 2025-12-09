using System;
using System.Collections.Generic;

namespace ShopHerePJ.Data.Entities;

public partial class category
{
    public int id { get; set; }

    public string name { get; set; } = null!;

    public string slug { get; set; } = null!;

    public int? parent_id { get; set; }

    public bool is_active { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual ICollection<category> Inverseparent { get; set; } = new List<category>();

    public virtual category? parent { get; set; }

    public virtual ICollection<product> products { get; set; } = new List<product>();
}
