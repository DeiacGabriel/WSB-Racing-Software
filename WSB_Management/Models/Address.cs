using System;
using System.Collections.Generic;

namespace WSB_Management.Models;

public partial class Address
{
    public long Id { get; set; }
    public string City { get; set; } = null!;
    public string Zip { get; set; } = null!;
    public string Street { get; set; } = null!;
    public Country Country { get; set; }
}
