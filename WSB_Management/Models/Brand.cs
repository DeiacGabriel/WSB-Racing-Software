using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WSB_Management.Models;

public class Brand
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ObservableCollection<Bike> Bikes { get; set; }
}
