using System;
using System.Collections.Generic;

namespace WSB_Racing.Models;

public partial class Address
{
    public long Id { get; set; }

    public long Customerid { get; set; }

    public long Countryid { get; set; }

    public string City { get; set; } = null!;

    public string Zip { get; set; } = null!;

    public string Street { get; set; } = null!;

    public virtual Country Country { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;
}
