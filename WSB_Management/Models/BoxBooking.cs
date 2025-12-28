using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WSB_Management.Models;

/// <summary>
/// Buchung einer Box für einen Kunden bei einem Event
/// </summary>
public class BoxBooking
{
    public long Id { get; set; }
    
    /// <summary>
    /// Die gebuchte Box
    /// </summary>
    [Required]
    public Box Box { get; set; } = null!;
    public long BoxId { get; set; }
    
    /// <summary>
    /// Der Kunde der die Box gebucht hat
    /// </summary>
    [Required]
    public Customer Customer { get; set; } = null!;
    public long CustomerId { get; set; }
    
    /// <summary>
    /// Das Event für das die Box gebucht wurde
    /// </summary>
    [Required]
    public Event Event { get; set; } = null!;
    public long EventId { get; set; }
    
    /// <summary>
    /// Datum der Buchung
    /// </summary>
    public DateTime BookingDate { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Preis der Boxbuchung
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }
    
    /// <summary>
    /// Ist die Boxbuchung bezahlt
    /// </summary>
    public bool IsPaid { get; set; } = false;
    
    /// <summary>
    /// Notizen zur Buchung
    /// </summary>
    public string? Notes { get; set; }
}
