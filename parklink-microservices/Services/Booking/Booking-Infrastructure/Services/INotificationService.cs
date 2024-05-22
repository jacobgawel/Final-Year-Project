using Booking_Domain.Data;
using Booking_Domain.Entities;

namespace Booking_Infrastructure.Services
{
    public interface INotificationService
    {
        public Task<bool> SendNewBookingEmail(string parkingAddress, BookingUpdateDto booking, string userEmail);
        public Task<bool> SendCancelledBookingRefundEmail(BookingRefund bookingRefundObj, BookingUpdateDto booking, string userEmail);
        public Task<bool> SendBookingRefundEmail(BookingRefund bookingRefundObj, BookingUpdateDto booking, string userEmail);
        public Task<bool> SendNoRefundEmail(string bookingEmail, string bookingId);
    }
}
