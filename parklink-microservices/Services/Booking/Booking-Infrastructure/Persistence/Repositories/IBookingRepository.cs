using Booking_Domain.Data;
using Booking_Domain.Entities;

namespace Booking_Infrastructure.Persistence.Repositories;

public interface IBookingRepository
{
    Task<List<Booking>> GetBooking();
    Task<List<Booking>> GetBookingForAccountId(Guid accountId);
    Task<List<BookingHumanizedDto>> GetHumanizedBookingsByAccountId(Guid accountId);
    Task<Booking?> GetBooking(Guid id);
    Task<Booking> CreateBooking(Booking booking, decimal price);
    Task<bool> UpdateBooking(BookingUpdateDto booking);
    Task<bool> DeleteBooking(Guid id);
    Task<bool> CheckIfSpotAvailable(Booking booking, int capacity);
    Task<bool> CheckSpotAvailableGrpc(Guid parkingId, DateTime dateTime, TimeSpan duration, int slotCapacity);
    Task<List<Booking>> GetFutureBookingForParkingId(Guid parkingId);
    Task<BookingPricing> GetPricingForPeriod(DateTime startDate, TimeSpan duration, decimal price);
    Task<List<BookingGapDto>> FindGapsForParking(Guid parkingId, DateTime bookingStart, DateTime bookingEnd, int capacity);
    Task<List<Booking>> GetBookingForProvider(Guid providerId);
    Task<BookingHumanizedDto> GetHumanizedBooking(Booking booking);
    Task<BookingAnalyticsDto> GetAnalyticsForProviderById(Guid providerId);
    Task<BookingRefundDto> ComputeRefundForBooking(Booking booking);
    Task<BookingRefund> UpdateBookingParkingDeleted(Guid bookingId);
    Task<BookingAnalyticsDto> GetAnalyticsForAdmin();
    Task<bool> SubmitRefund(BookingUpdateDto booking);

    // Booking Record repository functions
    Task<BookingRecord> CreateBookingRecord(BookingRecord bookingRecord);
    Task<BookingRecord?> GetBookingRecord(Guid id);
    Task<BookingRecord?> GetUnverifiedRecord(Guid id);
}