using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WSB_Management.Models;

/// <summary>
/// Preis pro Tag für ein Event - ermöglicht unterschiedliche Preise pro Wochentag
/// </summary>
public class EventDayPrice
{
    public long Id { get; set; }
    
    /// <summary>
    /// Das Event zu dem dieser Preis gehört
    /// </summary>
    [Required]
    public Event Event { get; set; } = null!;
    public long EventId { get; set; }
    
    /// <summary>
    /// Der Wochentag (0=Sonntag bis 6=Samstag) oder das spezifische Datum
    /// </summary>
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Standardpreis für diesen Tag
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal StandardPrice { get; set; } = 220.00m;
    
    /// <summary>
    /// Preis für Jahreskarten-Inhaber (meist 0)
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal SeasonPassPrice { get; set; } = 0.00m;
    
    /// <summary>
    /// Beschreibung/Bezeichnung des Tags (z.B. "Freitag Training")
    /// </summary>
    public string? Description { get; set; }
}
