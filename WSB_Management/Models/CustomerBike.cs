using System;
using System.Collections.Generic;

namespace WSB_Management.Models;

public class CustomerBike
{
    public long Id { get; set; }

    public long Customerid { get; set; }

    public long Bikeid { get; set; }

    public virtual Bike Bike { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;
}
