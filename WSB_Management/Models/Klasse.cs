using System;
using System.Collections.Generic;

namespace WSB_Management.Models;

public class Klasse
{
    public long Id { get; set; }
    public string Bezeichnung { get; set; } = string.Empty;
    
    // Navigation property
    public List<BikeType> BikeTypes { get; set; } = new();
}
