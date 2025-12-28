using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WSB_Management.Models;

/// <summary>
/// Jahreskarte eines Kunden
/// </summary>
public class SeasonPass
{
    public long Id { get; set; }
    
    /// <summary>
    /// Der Kunde der die Jahreskarte besitzt
    /// </summary>
    [Required]
    public Customer Customer { get; set; } = null!;
    public long CustomerId { get; set; }
    
    /// <summary>
    /// Das Jahr für das die Jahreskarte gilt
    /// </summary>
    public int Year { get; set; } = DateTime.Now.Year;
    
    /// <summary>
    /// Preis der Jahreskarte
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }
    
    /// <summary>
    /// Kaufdatum
    /// </summary>
    public DateTime PurchaseDate { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Ist die Jahreskarte bezahlt
    /// </summary>
    public bool IsPaid { get; set; } = false;
    
    /// <summary>
    /// Ist die Jahreskarte aktiv
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Notizen
    /// </summary>
    public string? Notes { get; set; }
}
