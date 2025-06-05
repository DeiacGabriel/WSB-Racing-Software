using System;
using System.Collections.Generic;

namespace WSB_Management.Models;

public class Contact
{
    public long Id { get; set; }

    public long Customerid { get; set; }

    public string? Firstname { get; set; }

    public string? Surname { get; set; }

    public string? Phonenumber { get; set; }

    public virtual Customer Customer { get; set; } = null!;
}
