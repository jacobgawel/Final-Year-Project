namespace Parking_Domain.Data;

public class ParkingHumanizedDto : ParkingUpdateDto
{
    public string HumanizedTimeLimit { get; set; }
    public string HumanizedCreatedDate { get; set; }
    public string HumanizedPricing { get; set; }
    public string HumanizedLastEditDate { get; set; }
    public string HumanizedVerifiedDate { get; set; }
    public string HumanizedLastEdit { get; set; }
    public string HumanizedCreatedAt { get; set; }
    public string HumanizedDayLimit { get; set; }
}