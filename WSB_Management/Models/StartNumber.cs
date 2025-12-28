using System.ComponentModel.DataAnnotations;

namespace WSB_Management.Models;

/// <summary>
/// Startnummer pro Event pro Kunde (kann sich pro Event ändern)
/// </summary>
public class StartNumber
{
    public long Id { get; set; }
    
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
    /// Die Startnummer für dieses Event
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string Number { get; set; } = string.Empty;
    
    /// <summary>
    /// Ist die Startnummer vergeben/belegt
    /// </summary>
    public bool IsAssigned { get; set; } = true;
    
    /// <summary>
    /// Datum der Zuweisung
    /// </summary>
    public DateTime AssignedDate { get; set; } = DateTime.Now;
}
