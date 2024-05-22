namespace Parking_Infrastructure.GrpcServices;

public interface IBookingGrpcServices
{
    /*message GetAvailableRequest {
        string BookingDate = 1;
        string Duration = 2;
        string ParkingId = 3;
        string SlotCapacity = 4;
    }*/
    Task<AvailableReturn> CheckAvailability(Guid parkingId, TimeSpan duration, DateTime bookingDate, int capacity);
}