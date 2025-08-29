using System;
using System.Collections.Generic;

namespace WSB_Management.Models;

public class CostumerEvent
{
    public long Id { get; set; }
    public TimeSpan Laptime { get; set; }
    public Bike? Bike { get; set; }
    public Customer Customer { get; set; } = null!;
    public Event Event { get; set; } = null!;
    public Transponder? Transponder { get; set; }
    
    /// <summary>
    /// Das Datum, an dem der Kunde am Event teilnimmt
    /// </summary>
    public DateTime ParticipationDate { get; set; }
    
    /// <summary>
    /// Status der Anmeldung
    /// </summary>
    public ParticipationStatus Status { get; set; } = ParticipationStatus.Registered;
    
    /// <summary>
    /// Datum der Anmeldung
    /// </summary>
    public DateTime RegistrationDate { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Besondere Bemerkungen
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Status einer Event-Teilnahme
/// </summary>
public enum ParticipationStatus
{
    Registered,      // Angemeldet
    Waitlist,        // Warteliste
    Absent,          // Abwesend
    AdditionalBike   // Zusatzbike
}
