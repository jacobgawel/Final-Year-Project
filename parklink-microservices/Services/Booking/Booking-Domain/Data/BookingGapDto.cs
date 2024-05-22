namespace Booking_Domain.Data;

public class BookingGapDto
{
    public Guid ParkingId { get; set; }
    public Dictionary<string, DateTime> BookingGaps { get; set; }
    public string HumanizedDateTime { get; set; }
}