using Parking_Domain.Entities;

namespace Parking_Infrastructure.Services;

public interface INotificationService
{
    public Task<bool> SendNewParkingEmail(Parking parking, string userEmail);
    public Task<bool> SendAltParkingEmail(string parkingId, Dictionary<string, string> bookingDetails);
    public Task<bool> SendParkingNotFound(string email);
    public Task<bool> SendParkingRejected(string email);
    public Task<bool> SendParkingVerified(string email);
}