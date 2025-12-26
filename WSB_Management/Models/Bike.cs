using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WSB_Management.Models;

public class Bike
{
    public long Id { get; set; }
    
    // Foreign Key
    public long? BikeTypeId { get; set; }
    
    // Navigation properties
    public BikeType? BikeType { get; set; }
    public List<Customer> Customers { get; set; } = new();
}
