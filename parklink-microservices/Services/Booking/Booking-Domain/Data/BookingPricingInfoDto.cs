using Booking_Domain.Entities;

namespace Booking_Domain.Data;

public class BookingPricingInfoDto : BookingPricing
{
    public Guid ParkingId { get; set; }
    public string HumanizedFees { get; set; }
    public string HumanizedTotal { get; set; }
    public string HumanizedSubTotal { get; set; }
}