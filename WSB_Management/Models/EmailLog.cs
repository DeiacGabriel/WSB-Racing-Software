using System.ComponentModel.DataAnnotations;

namespace WSB_Management.Models;

/// <summary>
/// Protokoll gesendeter Emails
/// </summary>
public class EmailLog
{
    public long Id { get; set; }
    
    /// <summary>
    /// Der Kunde an den gesendet wurde
    /// </summary>
    [Required]
    public Customer Customer { get; set; } = null!;
    public long CustomerId { get; set; }
    
    /// <summary>
    /// Das Event zu dem die Email gehört (optional)
    /// </summary>
    public Event? Event { get; set; }
    public long? EventId { get; set; }
    
    /// <summary>
    /// Empfänger Email-Adresse
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string RecipientEmail { get; set; } = string.Empty;
    
    /// <summary>
    /// Betreff der Email
    /// </summary>
    [MaxLength(500)]
    public string? Subject { get; set; }
    
    /// <summary>
    /// Art der Email
    /// </summary>
    public EmailType Type { get; set; } = EmailType.Confirmation;
    
    /// <summary>
    /// Sendedatum
    /// </summary>
    public DateTime SentDate { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Status des Versands
    /// </summary>
    public EmailStatus Status { get; set; } = EmailStatus.Sent;
    
    /// <summary>
    /// Notiz/Bemerkung zur Email
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Fehlertext bei Versandfehler
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Gesendet von (Personal)
    /// </summary>
    public string? SentBy { get; set; }
}

public enum EmailType
{
    Confirmation,       // Bestätigung
    WaitlistOffer,      // Wartelisten-Angebot
    Reminder,           // Erinnerung
    Invoice,            // Rechnung
    SeasonPass,         // Jahreskarte Info
    Newsletter,         // Newsletter
    Custom              // Manuell
}

public enum EmailStatus
{
    Pending,    // In Warteschlange
    Sent,       // Gesendet
    Failed,     // Fehlgeschlagen
    Bounced     // Zurückgewiesen
}
