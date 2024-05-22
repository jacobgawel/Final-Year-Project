using System.ComponentModel.DataAnnotations.Schema;

namespace Parking_Domain.Entities;

[Table("Parking")]
public class Parking
{
    /*
     * When adding new elements to the entity,
     * make sure to update the "Update" function in the parking repository
     */
    
    public Guid Id { get; set; }
    public string Address { get; set; }
    public string SlotType { get; set; }
    public string SlotSize { get; set; }
    public bool AvailabilityStatus { get; set; }
    public decimal Price { get; set; }
    public string? EVInfo { get; set; }
    public string AdditionalFeatures { get; set; }
    public bool TimeLimited { get; set; }
    public bool DayLimited { get; set; }
    public int DayLimit { get; set; }
    public TimeSpan TimeLimit { get; set; }
    public string? SlotNotes { get; set; }
    public string? SlotImages { get; set; }
    public int SlotCapacity { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public double? Longitude { get; set; }
    public string? City { get; set; }
    public double? Latitude { get; set; }
    public Guid AccountId { get; set; }
    public bool VerificationStatus { get; set; }
    public bool ParkingRejected { get; set; }
    public string Email { get; set; }
    public DateTime VerificationDate { get; set; }
    public DateTime LastEditDate { get; set; }
}