using System;
using System.Collections.Generic;

namespace WSB_Management.Models;

public class CustomerCup
{
    public long Id { get; set; }

    public long Customerid { get; set; }

    public long Cupid { get; set; }

    public virtual Cup Cup { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;
}
