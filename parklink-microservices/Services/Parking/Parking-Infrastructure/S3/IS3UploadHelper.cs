using Microsoft.AspNetCore.Http;
using Parking_Domain.Entities;
using Parking_Domain.Data;

namespace Parking_Infrastructure.S3
{
    public interface IS3UploadHelper
    {
        Task<ParkingImageDto> UploadToParklinkS3(List<IFormFile> files);
        Task<Parking> UpdateParkingImageS3(ParkingImageUpdateDto imageUpdateDto, Parking existingParking);
    }
}
