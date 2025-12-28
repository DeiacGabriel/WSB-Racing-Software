using System.ComponentModel.DataAnnotations;

namespace WSB_Management.Models;

/// <summary>
/// Cup/Wertung an der ein Kunde teilnehmen kann
/// </summary>
public class RaceCup
{
    public long Id { get; set; }
    
    /// <summary>
    /// Name des Cups (z.B. "WSB-Cup", "DUCATI-Challenge")
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Kurzbezeichnung
    /// </summary>
    [MaxLength(20)]
    public string? ShortName { get; set; }
    
    /// <summary>
    /// Beschreibung des Cups
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Jahr der Saison
    /// </summary>
    public int Year { get; set; } = DateTime.Now.Year;
    
    /// <summary>
    /// Ist der Cup aktiv
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Farbe für die Anzeige (Hex-Code)
    /// </summary>
    [MaxLength(7)]
    public string? Color { get; set; }
}
