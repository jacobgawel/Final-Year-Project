namespace Booking_Domain.Data;

public class BookingGapsRequestDto
{
    public DateTime BookingDate { get; set; }
    public DateTime BookingExit { get; set; }
}