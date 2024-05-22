namespace Reviews_Domain.Data;

public class ReviewCreateDto
{
    public string ReviewText { get; set; }
    public int ReviewRating { get; set; } = 1;
    public Guid ParkingId { get; set; }
}