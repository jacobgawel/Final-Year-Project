namespace Booking_Domain.Data;

public class BookingPriceDto
{
        // This is linked to the request that will be sent to check 
        // the pricing of the booking for a given period
        // ---- mapper then proceeds to map these values in the controller to 
        // ---- the BookingRecord instance
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid ParkingId { get; set; }
}