namespace Parking_Domain.Data;

public class ParkingUpdateDto
{
    public Guid Id { get; set; }
    public string Address { get; set; }
    public string SlotType { get; set; }
    public string SlotSize { get; set; }
    public bool AvailabilityStatus { get; set; }
    public decimal Price { get; set; }
    public string? EVInfo { get; set; }
    public string AdditionalFeatures { get; set; }
    public bool TimeLimited { get; set; }
    public TimeSpan TimeLimit { get; set; }
    public int DayLimit { get; set; }
    public bool DayLimited { get; set; }
    public string? SlotNotes { get; set; }
    public string? SlotImages { get; set; }
    public int SlotCapacity { get; set; }
    public double? Longitude { get; set; }
    public double? Latitude { get; set; }
    public string? City { get; set; }
    public bool VerificationStatus { get; set; }
    public bool ParkingRejected { get; set; }
}