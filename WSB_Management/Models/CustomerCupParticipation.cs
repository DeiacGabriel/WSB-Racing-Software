using System.ComponentModel.DataAnnotations;

namespace WSB_Management.Models;

/// <summary>
/// Teilnahme eines Kunden an einem Cup/einer Jahreswertung
/// </summary>
public class CustomerCupParticipation
{
    public long Id { get; set; }
    
    /// <summary>
    /// Der Kunde
    /// </summary>
    [Required]
    public Customer Customer { get; set; } = null!;
    public long CustomerId { get; set; }
    
    /// <summary>
    /// Der Cup
    /// </summary>
    [Required]
    public RaceCup RaceCup { get; set; } = null!;
    public long RaceCupId { get; set; }
    
    /// <summary>
    /// Ist die Teilnahme aktiv
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Registrierungsdatum
    /// </summary>
    public DateTime RegistrationDate { get; set; } = DateTime.Now;
}
