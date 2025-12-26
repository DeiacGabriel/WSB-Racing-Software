using System;
using System.Collections.Generic;

namespace WSB_Racing.Models;

public partial class Event
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime Validfrom { get; set; }

    public DateTime Validuntil { get; set; }

    public float? Vat { get; set; }

    public virtual ICollection<CustomerEvent> CustomerEvents { get; set; } = new List<CustomerEvent>();
}
