using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WSB_Management.Models;

public class Event
{
    public long Id { get; set; }
    public string Name { get; set; }
    public DateTime Validfrom { get; set; }
    public DateTime Validuntil { get; set; }
    public double Vat { get; set; }
}
