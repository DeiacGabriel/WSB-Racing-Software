using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WSB_Management.Models;

public class Event
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime Validfrom { get; set; }

    public DateTime Validuntil { get; set; }

    public double Vat { get; set; }

    public virtual ObservableCollection<CustomerEvent> CustomerEvents { get; set; }
}
