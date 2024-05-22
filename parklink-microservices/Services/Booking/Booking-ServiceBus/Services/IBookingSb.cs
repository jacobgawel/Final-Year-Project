using Booking_Domain.Topics;

namespace Booking_ServiceBus.Services
{
    public interface IBookingSb
    {
        public Task<bool> RecommendBooking(List<BookingConflict> bookings);
    }
}
