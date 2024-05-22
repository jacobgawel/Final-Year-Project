using Booking_Domain.Topics;
using Hangfire;
using MassTransit;
using Microsoft.Extensions.Logging;
using Parking_Infrastructure.GrpcServices;
using Parking_Infrastructure.Repositories;
using Parking_Infrastructure.Services;

namespace Parking_ServiceBus.Consumer;

public class BookingRecommendConsumer : IConsumer<BookingRecommend>
{
    private readonly ILogger<BookingRecommendConsumer> _logger;
    private readonly IBookingGrpcServices _bookingGrpcServices;
    private readonly IParkingRepository _repository;
    private readonly INotificationService _notificationService;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public BookingRecommendConsumer(ILogger<BookingRecommendConsumer> logger, IBookingGrpcServices bookingGrpcServices, IParkingRepository repository, INotificationService notificationService, IBackgroundJobClient backgroundJobClient)
    {
        _logger = logger;
        _bookingGrpcServices = bookingGrpcServices;
        _repository = repository;
        _notificationService = notificationService;
        _backgroundJobClient = backgroundJobClient;
    }

    public async Task Consume(ConsumeContext<BookingRecommend> context)
    {
        var recommendList = new List<Dictionary<string, string>>();
        var noAlternative = new List<string>();
        
        foreach (var booking in context.Message.Bookings)
        {
            _logger.LogInformation($"Parking Collision={booking.Email}, " +
                                   $"{booking.City}, {booking.Duration}, " +
                                   $"{booking.BookingDate}, Lat={booking.Longitude}, Long={booking.Latitude}");
            
            var parkingList = await _repository.GetParkingByCity(booking.City);

            bool foundAlternative = false;
            int i = 0;
            
            foreach (var parking in parkingList)
            {
                var recDict = new Dictionary<string, string>(); // recommend dict
                
                while (i != 3)
                {
                    var result =
                        await _bookingGrpcServices.CheckAvailability(parking.Id, booking.Duration, booking.BookingDate, parking.SlotCapacity);
                    _logger.LogInformation($"{booking.Email} --> {parking.Id} --> {result.Result}");
                    // if - { "result": true } it means it found one. if - { "result": false } it means it hasn't
                    if (result.Result)
                    {
                        recDict["email"] = booking.Email;
                        recDict["parkingId"] = parking.Id.ToString();
                        recDict["dateTime"] = booking.BookingDate.ToString();
                        recDict["duration"] = booking.Duration.ToString();
                        recommendList.Add(recDict);
                        foundAlternative = true;
                        break;
                    }

                    i++;

                    booking.BookingDate += TimeSpan.FromHours(1);
                }
                
                // breaks the loop immediately when an alternative slot is found
                if (foundAlternative)
                {
                    break;
                }
            }

            if (foundAlternative == false)
                noAlternative.Add(booking.Email);
        }

        foreach (var dict in recommendList)
        {
            _backgroundJobClient.Enqueue(() => _notificationService.SendAltParkingEmail(dict["parkingId"], dict));
            _logger.LogInformation($"{dict["email"]} --> {dict["parkingId"]} Date={dict["dateTime"]} Duration={dict["duration"]}");
        }

        foreach (var email in noAlternative)
        {
            _backgroundJobClient.Enqueue(() => _notificationService.SendParkingNotFound(email)); 
            _logger.LogInformation($"Parking spot has not been found for email: {email}, sending an email...");
        }
        
    }
}