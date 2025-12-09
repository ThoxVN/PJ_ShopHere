using System;
using System.Collections.Generic;

namespace ShopHerePJ.Data.Entities;

public partial class order
{
    public int id { get; set; }

    public string order_number { get; set; } = null!;

    public int? user_id { get; set; }

    public int? cart_id { get; set; }

    public string status { get; set; } = null!;

    public string payment_status { get; set; } = null!;

    public string? payment_method { get; set; }

    public DateTime? paid_at { get; set; }

    public string? shipping_method { get; set; }

    public string? tracking_number { get; set; }

    public DateTime? shipped_at { get; set; }

    public DateTime? delivered_at { get; set; }

    public int? shipping_address_id { get; set; }

    public int? billing_address_id { get; set; }

    public decimal subtotal_amount { get; set; }

    public decimal discount_amount { get; set; }

    public decimal shipping_fee { get; set; }

    public decimal tax_amount { get; set; }

    public decimal grand_total { get; set; }

    public string currency { get; set; } = null!;

    public string? customer_note { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual address? billing_address { get; set; }

    public virtual cart? cart { get; set; }

    public virtual ICollection<order_item> order_items { get; set; } = new List<order_item>();

    public virtual address? shipping_address { get; set; }

    public virtual user? user { get; set; }
}
