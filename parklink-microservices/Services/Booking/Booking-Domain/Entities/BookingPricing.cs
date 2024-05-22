namespace Booking_Domain.Entities;

public class BookingPricing
{
    // This is an object that is utilised to structure
    // the format of a BookingPricing
    public decimal Fees { get; set; }
    public decimal Total { get; set; }
    public decimal SubTotal { get; set; }
}