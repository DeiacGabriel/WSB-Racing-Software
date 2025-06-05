using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WSB_Management.Models;

public class Customer
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

    public virtual ObservableCollection<Address> Addresses { get; set; }

    public virtual ObservableCollection<Contact> Contacts { get; set; }

    public virtual ObservableCollection<CustomerBike> CustomerBikes { get; set; }

    public virtual ObservableCollection<CustomerCup> CustomerCups { get; set; }

    public virtual ObservableCollection<CustomerEvent> CustomerEvents { get; set; }
}
