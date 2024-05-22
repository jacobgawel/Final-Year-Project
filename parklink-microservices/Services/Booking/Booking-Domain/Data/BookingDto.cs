namespace Booking_Domain.Data;

public class BookingDto
{
    // This is linked to the Booking object. This is the request that the 
    // client will send to the booking endpoint to submit the booking
    // ---- mapper then proceeds to map these values in the controller to 
    // ---- the Booking instance (that's why these values are unused except the class)
    public string? CarRegistration { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid ParkingId { get; set; }
    public Guid RecordId { get; set; }
}