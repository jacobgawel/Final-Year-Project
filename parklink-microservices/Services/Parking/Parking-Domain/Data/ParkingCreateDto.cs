using Microsoft.AspNetCore.Http;

namespace Parking_Domain.Data
{
    public class ParkingCreateDto
    {
        public string Parking { get; set; }
        public List<IFormFile>? Files { get; set; }
    }
}
