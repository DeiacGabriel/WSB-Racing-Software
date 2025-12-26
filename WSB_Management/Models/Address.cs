using System;
using System.Collections.Generic;

namespace WSB_Management.Models;

public partial class Address
{
    public long Id { get; set; }
    public string City { get; set; } = string.Empty;
    public string Zip { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public Country? Country { get; set; }
}
