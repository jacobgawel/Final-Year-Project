using Microsoft.AspNetCore.Http;

namespace Fines_Domain.Data;

public class FineCreateDto
{
    public Guid BookingId { get; set; }
    public string Description { get; set; }
    public IFormFile File { get; set; }
}