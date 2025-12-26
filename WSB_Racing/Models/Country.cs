using System;
using System.Collections.Generic;

namespace WSB_Racing.Models;

public partial class Country
{
    public long Id { get; set; }

    public string Shorttxt { get; set; } = null!;

    public string Longtxt { get; set; } = null!;

    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
}
