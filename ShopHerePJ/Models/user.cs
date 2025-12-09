using System;
using System.Collections.Generic;

namespace ShopHerePJ.Data.Entities;

public partial class user
{
    public int userid { get; set; }

    public string email { get; set; } = null!;

    public string password_hash { get; set; } = null!;

    public string? full_name { get; set; }

    public string? phone { get; set; }

    public string role { get; set; } = null!;

    public string status { get; set; } = null!;

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual ICollection<address> addresses { get; set; } = new List<address>();

    public virtual ICollection<cart> carts { get; set; } = new List<cart>();

    public virtual ICollection<order> orders { get; set; } = new List<order>();

    public virtual ICollection<product_review> product_reviews { get; set; } = new List<product_review>();
}
