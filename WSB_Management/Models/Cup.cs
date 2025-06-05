using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WSB_Management.Models;

public class Cup
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ObservableCollection<CustomerCup> CustomerCups { get; set; }
}
