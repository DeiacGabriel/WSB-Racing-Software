using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WSB_Management.Models;

public class CostumerEvent
{
    public long Id { get; set; }
    
    /// <summary>
    /// Beste Rundenzeit bei diesem Event
    /// </summary>
    public TimeSpan Laptime { get; set; }
    
    /// <summary>
    /// Das Bike das bei diesem Event verwendet wird (kann vom Stammbike abweichen)
    /// </summary>
    public Bike? Bike { get; set; }
    public long? BikeId { get; set; }
    
    /// <summary>
    /// Der Kunde
    /// </summary>
    [Required]
    public Customer Customer { get; set; } = null!;
    public long CustomerId { get; set; }
    
    /// <summary>
    /// Das Event
    /// </summary>
    [Required]
    public Event Event { get; set; } = null!;
    public long EventId { get; set; }
    
    /// <summary>
    /// Der verwendete Transponder (kann geliehen sein)
    /// </summary>
    public Transponder? Transponder { get; set; }
    public long? TransponderId { get; set; }
    
    /// <summary>
    /// Sonderpreis für diesen Tag (überschreibt Standardpreis)
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal? SpecialPrice { get; set; }
    
    /// <summary>
    /// Ist dieser Tag mit Jahreskarte gebucht (Preis = 0)
    /// </summary>
    public bool IsSeasonPassBooking { get; set; } = false;
    
    /// <summary>
    /// Versicherung gebucht
    /// </summary>
    public bool HasInsurance { get; set; } = false;
    
    /// <summary>
    /// Transponder bezahlt (wenn geliehen)
    /// </summary>
    public bool TransponderPaid { get; set; } = false;
    
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
    
    /// <summary>
    /// Ist dieser Eintrag bezahlt
    /// </summary>
    public bool IsPaid { get; set; } = false;
    
    /// <summary>
    /// Betrag der bezahlt wurde
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal PaidAmount { get; set; } = 0;
    
    /// <summary>
    /// Startnummer für dieses Event (kann abweichen vom Kunden-Stamm)
    /// </summary>
    [MaxLength(10)]
    public string? EventStartNumber { get; set; }
    
    /// <summary>
    /// Anwesend markiert
    /// </summary>
    public bool IsPresent { get; set; } = false;
    
    /// <summary>
    /// Hilfseigenschaft: Farbcode für Status im Grid
    /// </summary>
    [NotMapped]
    public string StatusColor => Status switch
    {
        ParticipationStatus.Registered => IsPaid ? "success" : "primary",
        ParticipationStatus.Waitlist => "warning",
        ParticipationStatus.Absent => "secondary",
        ParticipationStatus.AdditionalBike => "info",
        _ => "secondary"
    };
    
    /// <summary>
    /// Hilfseigenschaft: CSS-Klasse für Zeile basierend auf Zahlungsstatus
    /// </summary>
    [NotMapped]
    public string RowClass
    {
        get
        {
            if (Status == ParticipationStatus.Absent) return "table-secondary";
            if (!IsPaid) return "table-warning";
            return "";
        }
    }
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
