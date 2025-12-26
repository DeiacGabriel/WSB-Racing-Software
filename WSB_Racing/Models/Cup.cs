using System;
using System.Collections.Generic;

namespace WSB_Racing.Models;

public partial class Cup
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<CustomerCup> CustomerCups { get; set; } = new List<CustomerCup>();
}
