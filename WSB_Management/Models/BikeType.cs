using System;
using System.Collections.Generic;

namespace WSB_Management.Models;

public class BikeType
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    
    // Foreign Keys
    public long BrandId { get; set; }
    public long KlasseId { get; set; }
    
    // Navigation properties
    public Brand Brand { get; set; } = null!;
    public Klasse Klasse { get; set; } = null!;
    public List<Bike> Bikes { get; set; } = new();
}
