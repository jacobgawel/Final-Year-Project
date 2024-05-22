using Hangfire;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Fines_Infrastructure.Services;

public class NotificationServices : INotificationServices
{
    [Queue("fines")]
    public async Task<bool> SendFineSubmittedEmail(string bookingId, string userEmail)
    {
        const string apiKey = "API_KEY_HERE";
        var client = new SendGridClient(apiKey);
        var from = new EmailAddress("support@cloudcore.cc", "Parklink Team");
        var to = new EmailAddress(userEmail);

        // template must match the data that is present on the dynamic template on SendGrid
        var dynamicTemplateData = new Dictionary<string, string> {
            { "email", userEmail },
            { "bookingId", bookingId}
        };

        var email = MailHelper.CreateSingleTemplateEmail(from, to, "d-4bc2eea309444d15886101149e65602d", dynamicTemplateData);
        var response = await client.SendEmailAsync(email);

        if (response.IsSuccessStatusCode) return true;

        throw new Exception("Email has failed to send: " + response.StatusCode);
    }
    
    [Queue("fines")]
    public async Task<bool> SendFineDeletedEmail(string bookingId, string userEmail)
    {
        const string apiKey = "API_KEY_HERE";
        var client = new SendGridClient(apiKey);
        var from = new EmailAddress("support@cloudcore.cc", "Parklink Team");
        var to = new EmailAddress(userEmail);

        // template must match the data that is present on the dynamic template on SendGrid
        var dynamicTemplateData = new Dictionary<string, string> {
            { "email", userEmail },
            { "bookingId", bookingId}
        };

        var email = MailHelper.CreateSingleTemplateEmail(from, to, "d-d7632f7339e240a5a09744552b678a65", dynamicTemplateData);
        var response = await client.SendEmailAsync(email);

        if (response.IsSuccessStatusCode) return true;

        throw new Exception("Email has failed to send: " + response.StatusCode);
    }
}