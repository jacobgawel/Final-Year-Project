using System.Globalization;
using Booking_Domain.Data;
using Booking_Domain.Entities;
using Hangfire;
using SendGrid.Helpers.Mail;
using SendGrid;

namespace Booking_Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        [Queue("booking")]
        public async Task<bool> SendNewBookingEmail(string parkingAddress, BookingUpdateDto booking, string userEmail)
        {
            const string apiKey = "API_KEY_HERE";
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("support@cloudcore.cc", "Parklink Team");
            var to = new EmailAddress(userEmail);
            
            var exit = booking.EndDate;

            // template must match the data that is present on the dynamic template on SendGrid
            var dynamicTemplateData = new Dictionary<string, string> {
                { "email", userEmail },
                { "arrival", $"{booking.StartDate.DayOfWeek}, {booking.StartDate:MMM} {booking.StartDate.Day}, {booking.StartDate.TimeOfDay}" },
                { "exit", $"{exit.DayOfWeek}, {booking.StartDate:MMM} {exit.Day}, {exit.TimeOfDay}" },
                { "parkingLocation", parkingAddress },
                { "bookingId", booking.Id.ToString() },
                { "subTotal", booking.SubTotal.ToString("0.00") },
                { "fees", booking.Fees.ToString("0.00") },
                { "total", booking.Total.ToString("0.00") },
                { "qrcode", booking.QrCodeLink! }
            };

            var email = MailHelper.CreateSingleTemplateEmail(from, to, "d-bd7b46ea448d437aafceee291970852f", dynamicTemplateData);
            var response = await client.SendEmailAsync(email);

            if (response.IsSuccessStatusCode) return true;

            throw new Exception("Email has failed to send: " + response.StatusCode);
        }

        [Queue("booking")]
        public async Task<bool> SendCancelledBookingRefundEmail(BookingRefund bookingRefundObj, BookingUpdateDto booking, string userEmail)
        {
            var refundId = bookingRefundObj.Id;
            var bookingRefund = bookingRefundObj.Total;
            
            const string apiKey = "API_KEY_HERE";
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("support@cloudcore.cc", "Parklink Team");
            var to = new EmailAddress(userEmail);
            
            // template must match the data that is present on the dynamic template on SendGrid
            var dynamicTemplateData = new Dictionary<string, string> {
                { "email", userEmail },
                { "bookingRefund", bookingRefund.ToString("C", new CultureInfo("en-GB")) },
                { "refundId", refundId.ToString() },
                { "bookingId", booking.Id.ToString() },
            };

            var email = MailHelper.CreateSingleTemplateEmail(from, to, "d-a5b6a5b669284709ab56047192200a36", dynamicTemplateData);
            var response = await client.SendEmailAsync(email);

            if (response.IsSuccessStatusCode) return true;

            throw new Exception("Email has failed to send: " + response.StatusCode);
        }
        
        [Queue("booking")]
        public async Task<bool> SendBookingRefundEmail(BookingRefund bookingRefundObj, BookingUpdateDto booking, string userEmail)
        {
            var refundId = bookingRefundObj.Id;
            var bookingRefund = bookingRefundObj.Total;
            
            const string apiKey = "API_KEY_HERE";
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("support@cloudcore.cc", "Parklink Team");
            var to = new EmailAddress(userEmail);
            
            // template must match the data that is present on the dynamic template on SendGrid
            var dynamicTemplateData = new Dictionary<string, string> {
                { "email", userEmail },
                { "bookingRefund", bookingRefund.ToString("C", new CultureInfo("en-GB")) },
                { "refundId", refundId.ToString() },
                { "bookingId", booking.Id.ToString() },
            };

            var email = MailHelper.CreateSingleTemplateEmail(from, to, "d-a5b6a5b669284709ab56047192200a36", dynamicTemplateData);
            var response = await client.SendEmailAsync(email);

            if (response.IsSuccessStatusCode) return true;

            throw new Exception("Email has failed to send: " + response.StatusCode);
        }
        
        [Queue("booking")]
        public async Task<bool> SendNoRefundEmail(string bookingEmail, string bookingId)
        {
            const string apiKey = "API_KEY_HERE";
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("support@cloudcore.cc", "Parklink Team");
            var to = new EmailAddress(bookingEmail);
            
            // template must match the data that is present on the dynamic template on SendGrid
            var dynamicTemplateData = new Dictionary<string, string> {
                { "email", bookingEmail },
                { "bookingId", bookingId },
            };

            var email = MailHelper.CreateSingleTemplateEmail(from, to, "d-87f50c506a1c4094996a4bdf3b686aae", dynamicTemplateData);
            var response = await client.SendEmailAsync(email);

            if (response.IsSuccessStatusCode) return true;

            throw new Exception("Email has failed to send: " + response.StatusCode);
        }
    }
}
