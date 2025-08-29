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
    
    /// <summary>
    /// Alle Anmeldungen zu diesem Event
    /// </summary>
    public List<CostumerEvent> CustomerEvents { get; set; } = new();
    
    public Event()
    {
        Validuntil = Validfrom.AddDays(2);
    }
    
    /// <summary>
    /// Gibt alle verfügbaren Tage dieses Events zurück
    /// </summary>
    public List<DateTime> GetEventDays()
    {
        var days = new List<DateTime>();
        var currentDate = Validfrom.Date;
        
        while (currentDate <= Validuntil.Date)
        {
            days.Add(currentDate);
            currentDate = currentDate.AddDays(1);
        }
        
        return days;
    }
    
    /// <summary>
    /// Gibt den deutschen Tagesnamen zurück
    /// </summary>
    public static string GetGermanDayName(DateTime date)
    {
        var germanDayNames = new Dictionary<DayOfWeek, string>
        {
            { DayOfWeek.Monday, "Montag" },
            { DayOfWeek.Tuesday, "Dienstag" },
            { DayOfWeek.Wednesday, "Mittwoch" },
            { DayOfWeek.Thursday, "Donnerstag" },
            { DayOfWeek.Friday, "Freitag" },
            { DayOfWeek.Saturday, "Samstag" },
            { DayOfWeek.Sunday, "Sonntag" }
        };
        
        return germanDayNames.ContainsKey(date.DayOfWeek) 
            ? germanDayNames[date.DayOfWeek] 
            : date.DayOfWeek.ToString();
    }
    
    private static DateTime GetNextFriday()
    {
        var today = DateTime.Today;
        int daysUntilFriday = ((int)DayOfWeek.Friday - (int)today.DayOfWeek + 7) % 7;
        return today.AddDays(daysUntilFriday);
    }
}
