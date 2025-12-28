using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WSB_Management.Models;

/// <summary>
/// Repräsentiert eine Zahlung/Transaktion eines Kunden
/// </summary>
public class Payment
{
    public long Id { get; set; }
    
    /// <summary>
    /// Der Kunde der gezahlt hat
    /// </summary>
    [Required]
    public Customer Customer { get; set; } = null!;
    public long CustomerId { get; set; }
    
    /// <summary>
    /// Das Event zu dem die Zahlung gehört (optional)
    /// </summary>
    public Event? Event { get; set; }
    public long? EventId { get; set; }
    
    /// <summary>
    /// Betrag der Zahlung
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Zahlungsmethode
    /// </summary>
    public PaymentMethod Method { get; set; } = PaymentMethod.Cash;
    
    /// <summary>
    /// Zahlungsdatum
    /// </summary>
    public DateTime PaymentDate { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Buchungsdatum (für Buchhaltung)
    /// </summary>
    public DateTime BookingDate { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Zahlungsart
    /// </summary>
    public PaymentType Type { get; set; } = PaymentType.EventParticipation;
    
    /// <summary>
    /// Beschreibung/Verwendungszweck
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Rechnungsnummer
    /// </summary>
    public string? InvoiceNumber { get; set; }
    
    /// <summary>
    /// Status der Zahlung
    /// </summary>
    public PaymentStatus Status { get; set; } = PaymentStatus.Completed;
    
    /// <summary>
    /// Fälligkeitsdatum (für Anzahlungen/Mahnungen)
    /// </summary>
    public DateTime? DueDate { get; set; }
    
    /// <summary>
    /// Erfasst von (Personal)
    /// </summary>
    public string? RecordedBy { get; set; }
    
    /// <summary>
    /// Notizen
    /// </summary>
    public string? Notes { get; set; }
}

public enum PaymentMethod
{
    Cash,           // Bar
    Card,           // Karte
    BankTransfer,   // Überweisung
    Hobix,          // Hobix System
    Credit,         // Guthaben
    SeasonPass      // Jahreskarte
}

public enum PaymentType
{
    EventParticipation,     // Eventteilnahme
    Deposit,                // Anzahlung
    BoxRental,              // Boxmiete
    Transponder,            // Transponder
    Insurance,              // Versicherung
    Shop,                   // Shop-Einkauf
    SeasonPass,             // Jahreskarte
    Refund,                 // Rückerstattung
    Credit,                 // Guthaben aufladen
    Other                   // Sonstiges
}

public enum PaymentStatus
{
    Pending,        // Ausstehend
    Completed,      // Bezahlt
    Reminded,       // Gemahnt
    Overdue,        // Überfällig
    Cancelled,      // Storniert
    Refunded        // Rückerstattet
}
