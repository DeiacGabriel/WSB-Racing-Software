using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WSB_Management.Models;

public class Customer
{
    public long Id { get; set; }
    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            if (value == "Herr")
                Sex = "M";
            else if (value == "Frau")
                Sex = "F";
            else
                Sex = string.Empty;
        }
    }
    public string Sex { get; private set; } = string.Empty;
    public Contact Contact { get; set; }
    public DateTime Birthdate { get; set; }
    public int Age
    {
        get
        {
            var today = DateTime.Today;
            var age = today.Year - Birthdate.Year;
            if (Birthdate.Date > today.AddYears(-age))
                age--;

            return age;
        }
    }
    public string Mail { get; set; }
    public bool Newsletter { get; set; }
    public DateTime Validfrom { get; set; }
    public string Startnumber { get; set; }
    public Address Address { get; set; }
    public Contact NotfallContact { get; set; }
    public string Sponsor { get; set; }
    public string UID { get; set; }
    public double Guthaben { get; set; }
    public DateTime LastGuthabenAdd { get; set; }
    public double LastGuthabenAddNumber { get; set; }
    public string GuthabenComment { get; set; }
    public double Preisgeld { get; set; }
    public double Gratisfahrer { get; set; }
    public double Schurke { get; set; }
    public bool VerzichtOk { get; set; }
    public Gruppe Gruppe { get; set; }
    public DateTime letzteBuchung { get; set; }
    public DateTime letzterEinkauf { get; set; }
    public Bike Bike { get; set; }
    public Transponder Transponder { get; set; }
    public TimeSpan? BestTime { get; set; } // Beste Zeit des Kunden für Gruppeneinteilung
    public string S8S { get; set; }
    public string Speeddays { get; set; }
    public Customer()
    {
        Gruppe = new Gruppe();
        Bike = new Bike();
        Contact = new Contact();
        Address = new Address();
        NotfallContact = new Contact();
        Transponder = new Transponder();
        GuthabenComment = string.Empty;
        Title = string.Empty;
        Mail = string.Empty;
        Startnumber = string.Empty;
        Sponsor = string.Empty;
        UID = string.Empty;
        S8S = string.Empty;
        Speeddays = string.Empty;
    }
}
