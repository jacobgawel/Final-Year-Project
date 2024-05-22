namespace Booking_Domain.Data;

public class BookingUpdateDto
{
    /*
     * BookingUpdateDto must be used to update the entity using the
     * UpdateBooking method. The values that want to be updated in the Booking must be added here.
     * Mapper can be used when changing values after fetching the original values from the database
     */
    public Guid Id { get; set; }
    public string CarRegistration { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool FineStatus { get; set; }
    public DateTime LastUpdated { get; set; }
    public bool BookingConfirmation { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Fees { get; set; }
    public decimal Total { get; set; }
    public string? QrCodeLink { get; set; } // this is here due to s3 handler updating
    // the entity in a background job after creation,
    // so it must be updated rather than created
}