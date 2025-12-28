using System.ComponentModel.DataAnnotations;

namespace WSB_Management.Models;

/// <summary>
/// Verzichtserklärung eines Kunden - einmal pro Jahr erforderlich
/// </summary>
public class WaiverDeclaration
{
    public long Id { get; set; }
    
    /// <summary>
    /// Der Kunde der die Verzichtserklärung abgegeben hat
    /// </summary>
    [Required]
    public Customer Customer { get; set; } = null!;
    public long CustomerId { get; set; }
    
    /// <summary>
    /// Das Jahr für das die Verzichtserklärung gilt
    /// </summary>
    public int Year { get; set; } = DateTime.Now.Year;
    
    /// <summary>
    /// Datum der Abgabe
    /// </summary>
    public DateTime SignedDate { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Art der Abgabe
    /// </summary>
    public WaiverType Type { get; set; } = WaiverType.InPerson;
    
    /// <summary>
    /// Erfasst von (Personal)
    /// </summary>
    public string? RecordedBy { get; set; }
    
    /// <summary>
    /// Ist die Verzichtserklärung gültig
    /// </summary>
    public bool IsValid { get; set; } = true;
    
    /// <summary>
    /// Notizen
    /// </summary>
    public string? Notes { get; set; }
}

public enum WaiverType
{
    InPerson,       // Persönlich abgegeben
    Online,         // Online unterschrieben
    Mail,           // Per Post
    Email           // Per Email (Scan)
}
