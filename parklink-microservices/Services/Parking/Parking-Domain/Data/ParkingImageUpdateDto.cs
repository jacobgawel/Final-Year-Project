using Microsoft.AspNetCore.Http;

namespace Parking_Domain.Data;

public class ParkingImageUpdateDto
{
    public Guid ParkingId { get; set; }
    public List<IFormFile>? ImageList { get; set; }
    public List<string>? DeleteList { get; set; }
}