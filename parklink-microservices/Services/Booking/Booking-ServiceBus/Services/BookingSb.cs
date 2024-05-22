using Booking_Domain.Entities;
using Booking_Domain.Topics;
using MassTransit;

namespace Booking_ServiceBus.Services
{
    public class BookingSb : IBookingSb
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public BookingSb(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }


        public async Task<bool> RecommendBooking(List<BookingConflict> bookings)
        {
            BookingRecommend bookingRecommend = new BookingRecommend
            {
                Bookings = bookings,
                Event = "Recommendations"
            };

            await _publishEndpoint.Publish(bookingRecommend);

            return true;
        }
    }
}
