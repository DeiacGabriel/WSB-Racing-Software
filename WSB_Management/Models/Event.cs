using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WSB_Management.Models;

public class Event
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Validfrom { get; set; } = GetNextFriday();
    public DateTime Validuntil { get; set; }
    public int maxPersons { get; set; } = 100;
    public double Vat { get; set; } = 20.0;
    public Event()
    {
        Validuntil = Validfrom.AddDays(2);
    }
    private static DateTime GetNextFriday()
    {
        var today = DateTime.Today;
        int daysUntilFriday = ((int)DayOfWeek.Friday - (int)today.DayOfWeek + 7) % 7;
        return today.AddDays(daysUntilFriday);
    }
}
