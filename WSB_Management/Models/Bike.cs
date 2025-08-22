using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WSB_Management.Models;

public class Bike
{
    public long Id { get; set; }
    public string Type { get; set; }
    public string Ccm { get; set; }
    public int Year { get; set; }
    public Brand Brand { get; set; }
    public List<Customer> Customers { get; set; }
}
