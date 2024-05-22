using Hangfire;
using Newtonsoft.Json;
using Parking_Domain.Entities;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Parking_Infrastructure.Services;

public class NotificationService : INotificationService
{
    [Queue("parking")]
    public async Task<bool> SendNewParkingEmail(Parking parking, string userEmail)
    {
        /*
         * This function is responsible for sending the confirmation email to the user that just books a parking spot.
         * The function utilises the dynamic email templates from sendgrid which are populated dynamically using JSON/Dictionary
         */
        
        const string apiKey = "API_KEY_HERE";
        var client = new SendGridClient(apiKey);
        var from = new EmailAddress("support@cloudcore.cc", "Parklink Team");
        var to = new EmailAddress(userEmail);
        
        var dynamicTemplateData = new Dictionary<string, string> {
            { "userEmail", userEmail }
        };

        var email = MailHelper.CreateSingleTemplateEmail(from, to, "d-95c79afc47f341b7964f4487ee68f5e1", dynamicTemplateData);
        var response = await client.SendEmailAsync(email);
        
        if (response.IsSuccessStatusCode) return true;
        
        throw new Exception("Email has failed to send: " + response.StatusCode);
    }

    [Queue("parking")]
    public async Task<bool> SendAltParkingEmail(string parkingId, Dictionary<string, string> bookingDetails)
    {
        const string apiKey = "API_KEY_HERE";
        var client = new SendGridClient(apiKey);
        var from = new EmailAddress("support@cloudcore.cc", "Parklink Team");
        var to = new EmailAddress(bookingDetails["email"]);

        var dateTime = DateTime.Parse(bookingDetails["dateTime"]);
        var dateString = $"{dateTime.DayOfWeek}, {dateTime:MMMM} {dateTime.Date.Day}, {dateTime.TimeOfDay}";
        
        var dynamicTemplateData = new Dictionary<string, string> {
            { "dateTime", dateString },
            { "parkingId", parkingId },
        };
        
        var email = MailHelper.CreateSingleTemplateEmail(from, to, "d-6819347032754070979768c99c1d72af", dynamicTemplateData);
        var response = await client.SendEmailAsync(email);
        
        if (response.IsSuccessStatusCode) return true;
        
        throw new Exception("Email has failed to send: " + response.StatusCode);
    }
    
    [Queue("parking")]
    public async Task<bool> SendParkingNotFound(string userEmail)
    {
        const string apiKey = "API_KEY_HERE";
        var client = new SendGridClient(apiKey);
        var from = new EmailAddress("support@cloudcore.cc", "Parklink Team");
        var to = new EmailAddress(userEmail);
        
        var dynamicTemplateData = new Dictionary<string, string> {
            { "userEmail", userEmail }
        };

        var email = MailHelper.CreateSingleTemplateEmail(from, to, "d-be9391bca60d4a9297faf30d5d090242", dynamicTemplateData);
        var response = await client.SendEmailAsync(email);
        
        if (response.IsSuccessStatusCode) return true;
        
        throw new Exception("Email has failed to send: " + response.StatusCode);
    }

    public async Task<bool> SendParkingRejected(string userEmail)
    {
        const string apiKey = "API_KEY_HERE";
        var client = new SendGridClient(apiKey);
        var from = new EmailAddress("support@cloudcore.cc", "Parklink Team");
        var to = new EmailAddress(userEmail);
        
        var dynamicTemplateData = new Dictionary<string, string> {
            { "userEmail", userEmail }
        };

        var email = MailHelper.CreateSingleTemplateEmail(from, to, "d-941e7d1c1ef3403f8406aa07e4a6a13e", dynamicTemplateData);
        var response = await client.SendEmailAsync(email);
        
        if (response.IsSuccessStatusCode) return true;
        
        throw new Exception("Email has failed to send: " + response.StatusCode);
    }

    public async Task<bool> SendParkingVerified(string userEmail)
    {
        const string apiKey = "API_KEY_HERE";
        var client = new SendGridClient(apiKey);
        var from = new EmailAddress("support@cloudcore.cc", "Parklink Team");
        var to = new EmailAddress(userEmail);
        
        var dynamicTemplateData = new Dictionary<string, string> {
            { "userEmail", userEmail }
        };

        var email = MailHelper.CreateSingleTemplateEmail(from, to, "d-8ce4d98814bc47f0aea64b2c36ceef92", dynamicTemplateData);
        var response = await client.SendEmailAsync(email);
        
        if (response.IsSuccessStatusCode) return true;
        
        throw new Exception("Email has failed to send: " + response.StatusCode);
    }
}