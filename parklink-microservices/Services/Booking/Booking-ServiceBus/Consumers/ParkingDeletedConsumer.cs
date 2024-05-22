using AutoMapper;
using Booking_Domain.Data;
using Booking_Domain.Entities;
using Booking_Domain.Topics;
using Booking_Infrastructure.Persistence.Repositories;
using Booking_Infrastructure.Services;
using Booking_ServiceBus.Services;
using Hangfire;
using MassTransit;
using Microsoft.Extensions.Logging;
using Parking_Domain.Topics;

namespace Booking_ServiceBus.Consumers
{

    public class ParkingDeletedConsumer : IConsumer<ParkingDeleted>
    {
        private readonly ILogger<ParkingDeletedConsumer> _logger;
        private readonly IBookingRepository _repository;
        private readonly IBookingSb _bookingSb;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;

        public ParkingDeletedConsumer(ILogger<ParkingDeletedConsumer> logger, 
            IBookingRepository repository, IBookingSb bookingSb, IBackgroundJobClient backgroundJobClient, INotificationService notificationService, IMapper mapper)
        {
            _logger = logger;
            _repository = repository;
            _bookingSb = bookingSb;
            _backgroundJobClient = backgroundJobClient;
            _notificationService = notificationService;
            _mapper = mapper;
        }

        public async Task Consume(ConsumeContext<ParkingDeleted> context)
        {
            /*
             * 
             * This function is responsible for consuming the ParkingDeleted event, making the bookings unavailable
             * that are going to be cancelled and then sending the same bookings to the BookingRecommendConsumer.
             * The booking recommend consumer will then send potential recommended parking spots for the user.
             * 
             */
            _logger.LogInformation($"Consuming ---> Parking Id={context.Message.Parking.Id}, Event={context.Message.Event}");

            var bookingConflicts = await _repository.GetFutureBookingForParkingId(context.Message.Parking.Id);

            foreach (var booking in bookingConflicts)
            {
                // TODO: Separate function to refund a booking fully (minus the transaction fees)
                var bookingRefund = await _repository.UpdateBookingParkingDeleted(booking.Id);
                var updateDto = new BookingUpdateDto();
                _mapper.Map(booking, updateDto);
                _backgroundJobClient.Enqueue(() => _notificationService.SendCancelledBookingRefundEmail(bookingRefund, updateDto, booking.Email));
            }
            List<BookingConflict> bookings = bookingConflicts.Select(booking => new BookingConflict
            {
                BookingDate = booking.StartDate, 
                Duration = booking.EndDate - booking.StartDate,
                Email = booking.Email,
                City = context.Message.Parking.City!,
                Longitude = context.Message.Parking.Longitude,
                Latitude = context.Message.Parking.Latitude
            }).ToList();

            await _bookingSb.RecommendBooking(bookings);
        }
    }
}
