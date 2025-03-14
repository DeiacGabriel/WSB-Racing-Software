using System;
using System.Collections.Generic;

namespace WSB_Racing.Models;

public partial class CustomerEvent
{
    public long Id { get; set; }

    public long Customerid { get; set; }

    public long Eventid { get; set; }

    public long Bikeid { get; set; }

    public long Transponderid { get; set; }

    public TimeSpan? Laptime { get; set; }

    public virtual Bike Bike { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;

    public virtual Event Event { get; set; } = null!;

    public virtual Transponder Transponder { get; set; } = null!;
}
