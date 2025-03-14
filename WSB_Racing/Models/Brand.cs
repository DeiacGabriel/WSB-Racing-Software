using System;
using System.Collections.Generic;

namespace WSB_Racing.Models;

public partial class Brand
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Bike> Bikes { get; set; } = new List<Bike>();
}
