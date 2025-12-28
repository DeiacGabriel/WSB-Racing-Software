using System.ComponentModel.DataAnnotations;

namespace WSB_Management.Models;

/// <summary>
/// Repräsentiert eine Garagenbox die bei Events gebucht werden kann
/// </summary>
public class Box
{
    public long Id { get; set; }
    
    /// <summary>
    /// Boxnummer (z.B. "A1", "B12")
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string Number { get; set; } = string.Empty;
    
    /// <summary>
    /// Bezeichnung/Name der Box
    /// </summary>
    [MaxLength(100)]
    public string? Name { get; set; }
    
    /// <summary>
    /// Ist dies eine Sammelbox (für mehrere Personen)
    /// </summary>
    public bool IsSammelbox { get; set; } = false;
    
    /// <summary>
    /// Maximale Kapazität der Box (bei Sammelboxen)
    /// </summary>
    public int MaxCapacity { get; set; } = 1;
    
    /// <summary>
    /// Standort/Bereich der Box
    /// </summary>
    [MaxLength(50)]
    public string? Location { get; set; }
    
    /// <summary>
    /// Notizen zur Box
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Ist die Box aktiv/verfügbar
    /// </summary>
    public bool IsActive { get; set; } = true;
}
