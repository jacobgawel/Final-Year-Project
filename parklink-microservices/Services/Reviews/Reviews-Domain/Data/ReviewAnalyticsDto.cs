using Reviews_Domain.Entities;

namespace Reviews_Domain.Data;

public class ReviewAnalyticsDto
{
    public double ParkingRating { get; set; }
    public List<Review> Reviews { get; set; }
}