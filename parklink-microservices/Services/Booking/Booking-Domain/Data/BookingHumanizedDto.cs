namespace Booking_Domain.Data;

public class BookingHumanizedDto : BookingUpdateDto
{
    // This entity is used as a return to the client with more humanized essential details (card component)
    // this inherits the BookingUpdateRequest to make sure that the client has the necessary details
    // to update on the card, e.g., cancel booking
    
    // DON'T DELETE: Parking id is used to control the map on the MyBookings page (on the frontend)
    public Guid ParkingId { get; set; }
    public Guid ProviderId { get; set; }
    public string Email { get; set; }
    public string AccountId { get; set; }
    public Guid RecordId { get; set; }
    
    // refund details (if there is one)
    public bool RefundStatus { get; set; }
    public decimal RefundAmount { get; set; }
    
    // Humanized fields
    public string HumanizedDate { get; set; }
    public string HumanizedDuration { get; set; }
    public string HumanizedTotal { get; set; }
    public string HumanizedSubTotal { get; set; }
    public string HumanizedFees { get; set; }
    public string HumanizedCreatedAt { get; set; }
    public string HumanizedBookingSpan { get; set; }
    public string HumanizedCreatedAtDate { get; set; }
    public string HumanizedLastUpdated { get; set; }
    public string HumanizedRefundAmount { get; set; }
}