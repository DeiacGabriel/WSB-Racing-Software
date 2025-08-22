using System;
using System.Collections.Generic;

namespace WSB_Management.Models;

public class CostumerEvent
{
    public long Id { get; set; }
    public TimeSpan Laptime { get; set; }
    public Bike Bike { get; set; }
    public Customer Customer { get; set; }
    public Event Event { get; set; }
    public Transponder Transponder { get; set; }
}
