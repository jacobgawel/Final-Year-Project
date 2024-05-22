using Parking_Domain.Entities;

namespace Parking_Domain.Data;

public class ParkingDistanceDto
{
    public Parking Parking { get; set; }
    public double Distance { get; set; }
    public bool AvailabilityStatus { get; set; }
}