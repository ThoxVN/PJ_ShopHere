using ShopHerePJ.Models;
using System;
using System.Collections.Generic;

namespace ShopHerePJ.Data.Entities;

public partial class cart
{
    public int id { get; set; }

    public int? user_id { get; set; }

    public string? session_id { get; set; }

    public string currency { get; set; } = null!;

    public string status { get; set; } = null!;

    public decimal subtotal_amount { get; set; }

    public decimal discount_amount { get; set; }

    public decimal shipping_fee { get; set; }

    public decimal tax_amount { get; set; }

    public decimal grand_total { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual ICollection<cart_item> cart_items { get; set; } = new List<cart_item>();

    public virtual ICollection<order> orders { get; set; } = new List<order>();

    public virtual user? user { get; set; }
}
