namespace Fines_Infrastructure.Services;

public interface INotificationServices
{
    Task<bool> SendFineSubmittedEmail(string bookingId, string userEmail);
    Task<bool> SendFineDeletedEmail(string bookingId, string userEmail);
}