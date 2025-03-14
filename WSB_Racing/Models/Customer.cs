using System;
using System.Collections.Generic;

namespace WSB_Racing.Models;

public partial class Customer
{
    public long Id { get; set; }

    public string? Title { get; set; }

    public string Firstname { get; set; } = null!;

    public string Surname { get; set; } = null!;

    public DateTime Birthdate { get; set; }

    public string? Sex { get; set; }

    public string? Mail { get; set; }

    public string? Phonenumber { get; set; }

    public bool? Newsletter { get; set; }

    public DateTime? Validfrom { get; set; }

    public string? Startnumber { get; set; }

    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();

    public virtual ICollection<Contact> Contacts { get; set; } = new List<Contact>();

    public virtual ICollection<CustomerBike> CustomerBikes { get; set; } = new List<CustomerBike>();

    public virtual ICollection<CustomerCup> CustomerCups { get; set; } = new List<CustomerCup>();

    public virtual ICollection<CustomerEvent> CustomerEvents { get; set; } = new List<CustomerEvent>();
}
