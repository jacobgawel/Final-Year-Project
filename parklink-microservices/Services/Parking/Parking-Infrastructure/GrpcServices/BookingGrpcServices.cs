using System.Globalization;

namespace Parking_Infrastructure.GrpcServices;

public class BookingGrpcServices : IBookingGrpcServices
{
    private readonly BookingProtoService.BookingProtoServiceClient _bookingProtoService;

    public BookingGrpcServices(BookingProtoService.BookingProtoServiceClient bookingProtoService)
    {
        _bookingProtoService = bookingProtoService;
    }

    public async Task<AvailableReturn> CheckAvailability(Guid parkingId, TimeSpan duration, DateTime bookingDate, int capacity)
    {
        var checkRequest = new GetAvailableRequest
        {
            ParkingId = parkingId.ToString(),
            Duration = duration.ToString(),
            BookingDate = bookingDate.ToUniversalTime().ToString(),
            SlotCapacity = capacity
        };
        var result = await _bookingProtoService.CheckAvailableAsync(checkRequest);
        return result;
    }
}