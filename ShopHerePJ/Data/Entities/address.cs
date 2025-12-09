using System;
using System.Collections.Generic;

namespace ShopHerePJ.Data.Entities;

public partial class address
{
    public int id { get; set; }

    public int? user_id { get; set; }

    public string type { get; set; } = null!;

    public string? recipient_name { get; set; }

    public string? phone { get; set; }

    public string? street { get; set; }

    public string? ward { get; set; }

    public string? district { get; set; }

    public string? city { get; set; }

    public string? postal_code { get; set; }

    public string country { get; set; } = null!;

    public bool is_default { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual ICollection<order> orderbilling_addresses { get; set; } = new List<order>();

    public virtual ICollection<order> ordershipping_addresses { get; set; } = new List<order>();

    public virtual user? user { get; set; }
}
