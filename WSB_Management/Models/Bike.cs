using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WSB_Management.Models;

public class Bike
{
    public long Id { get; set; }

    public long Brandid { get; set; }

    public string? Type { get; set; }

    public int? Ccm { get; set; }

    public int? Year { get; set; }

    public virtual Brand Brand { get; set; } = null!;

    public virtual ObservableCollection<CustomerBike> CustomerBikes { get; set; }

    public virtual ObservableCollection<CustomerEvent> CustomerEvents { get; set; }
}
