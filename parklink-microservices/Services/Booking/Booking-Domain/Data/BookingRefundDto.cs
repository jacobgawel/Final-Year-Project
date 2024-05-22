namespace Booking_Domain.Data;

public class BookingRefundDto
{
    public string RefundAmount { get; set; }
    public string RefundPercentage { get; set; }
    public Guid BookingId { get; set; }
    public bool FullRefund { get; set; }
    public bool NoRefund { get; set; }
    public decimal TotalRefund { get; set; }
    
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
}