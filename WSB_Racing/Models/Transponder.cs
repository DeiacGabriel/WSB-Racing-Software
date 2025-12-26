using System;
using System.Collections.Generic;

namespace WSB_Racing.Models;

public partial class Transponder
{
    public long Id { get; set; }

    public string Number { get; set; } = null!;

    public virtual ICollection<CustomerEvent> CustomerEvents { get; set; } = new List<CustomerEvent>();
}
