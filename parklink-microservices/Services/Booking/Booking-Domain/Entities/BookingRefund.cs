namespace Booking_Domain.Entities;

public class BookingRefund
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public decimal Total { get; set; }
    public Guid RecordId { get; set; }
    public Guid ProviderId { get; set; }
    public Guid AccountId { get; set; }
}