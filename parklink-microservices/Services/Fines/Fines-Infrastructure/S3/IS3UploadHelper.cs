using Fines_Domain.Data;
using Microsoft.AspNetCore.Http;

namespace Fines_Infrastructure.S3;

public interface IS3UploadHelper
{
    Task<FineImageDto> UploadToParklinkS3(IFormFile file, string bookingId);
}