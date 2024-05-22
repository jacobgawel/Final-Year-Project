using System.ComponentModel.DataAnnotations.Schema;

namespace Reviews_Domain.Entities;

[Table("Review")]
public class Review
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string ReviewTitle { get; set; }
    public string ReviewText { get; set; }
    public int ReviewRating { get; set; } = 1;
    public Guid ParkingId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime EditDate { get; set; } = DateTime.UtcNow;
}