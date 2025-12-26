using System;
using System.Collections.Generic;

namespace WSB_Racing.Models;

public partial class Bike
{
    public long Id { get; set; }

    public long Brandid { get; set; }

    public string? Type { get; set; }

    public int? Ccm { get; set; }

    public int? Year { get; set; }

    public virtual Brand Brand { get; set; } = null!;

    public virtual ICollection<CustomerBike> CustomerBikes { get; set; } = new List<CustomerBike>();

    public virtual ICollection<CustomerEvent> CustomerEvents { get; set; } = new List<CustomerEvent>();
}
