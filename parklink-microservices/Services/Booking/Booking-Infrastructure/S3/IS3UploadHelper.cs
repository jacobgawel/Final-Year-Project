namespace Booking_Infrastructure.S3;

public interface IS3UploadHelper
{
    Task<bool> UploadQrCode(Guid bookingId, string address);
}