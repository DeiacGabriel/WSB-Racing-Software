using System.ComponentModel.DataAnnotations;

namespace WSB_Management.Models;

/// <summary>
/// Referenz-Rundenzeit eines Kunden auf einer Strecke
/// </summary>
public class LaptimeReference
{
    public long Id { get; set; }
    
    /// <summary>
    /// Der Kunde
    /// </summary>
    [Required]
    public Customer Customer { get; set; } = null!;
    public long CustomerId { get; set; }
    
    /// <summary>
    /// Das Event/die Strecke
    /// </summary>
    [Required]
    public Event Event { get; set; } = null!;
    public long EventId { get; set; }
    
    /// <summary>
    /// Die beste Rundenzeit
    /// </summary>
    public TimeSpan BestLaptime { get; set; }
    
    /// <summary>
    /// Datum der Bestzeit
    /// </summary>
    public DateTime RecordDate { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Jahr der Referenz (für Vorjahresvergleich)
    /// </summary>
    public int Year { get; set; } = DateTime.Now.Year;
    
    /// <summary>
    /// Ist diese Zeit verifiziert (z.B. durch Transponder)
    /// </summary>
    public bool IsVerified { get; set; } = false;
    
    /// <summary>
    /// Notizen
    /// </summary>
    public string? Notes { get; set; }
}
