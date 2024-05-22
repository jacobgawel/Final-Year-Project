using System.Globalization;
using Booking_Domain.Entities;
using Booking_Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AutoMapper;
using Booking_Domain.Data;
using Booking_Infrastructure.Services;
using Humanizer;
using Itenso.TimePeriod;

namespace Booking_Infrastructure.Persistence.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly BookingDbContext _context;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;
    public BookingRepository(BookingDbContext context, IMapper mapper, 
        INotificationService notificationService)
    {
        _context = context;
        _mapper = mapper;
        _notificationService = notificationService;
    }
    
    
    // ------------------ BOOKING RECORD REPOSITORIES ---------------------

    public async Task<BookingRecord> CreateBookingRecord(BookingRecord bookingRecord)
    {
        var bookingRecordId = Guid.NewGuid();
        bookingRecord.Id = bookingRecordId;
        bookingRecord.TransactionDate = DateTime.UtcNow;

        await _context.BookingRecords.AddAsync(bookingRecord);
        await _context.SaveChangesAsync();

        return bookingRecord;
    }

    public async Task<BookingRecord?> GetBookingRecord(Guid id)
    {
        var bookingRecord = await _context.BookingRecords.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);
        return bookingRecord;
    }

    public async Task<BookingRecord?> GetUnverifiedRecord(Guid id)
    {
        var bookingRecord = await _context.BookingRecords.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Verified == false && b.Id == id);

        return bookingRecord;
    }
    
    // ---------------- END BOOKING RECORD ---------------
    
    
    
    // --------------- BOOKING REPOSITORIES --------------
    
    // temp repo (finding gaps)

    public async Task<List<BookingGapDto>> FindGapsForParking(Guid parkingId, DateTime bookingStart, DateTime bookingEnd, int capacity)
    {
        // fetch existing bookings
        var existingBookings = await _context.Bookings
            .Where(b => b.StartDate.Date == bookingStart.Date 
                        && b.ParkingId == parkingId && b.BookingConfirmation == true).ToListAsync();

        var bookingGaps = new List<BookingGapDto>();
        var targetTime = 23 - bookingStart.Hour;
        
        var duration = bookingEnd - bookingStart;
        for (int i = 1; i < targetTime; i++)
        {
            var newBookingStart = bookingStart.AddHours(i);
            var newBookingEnd = newBookingStart.Add(duration);
            var result = CheckBooking(existingBookings, newBookingStart, newBookingEnd, duration, capacity);
            if (result)
            {
                var bookingGapDto = new BookingGapDto
                {
                    ParkingId = parkingId,
                    BookingGaps = new Dictionary<string, DateTime>
                    {
                        { "startDate", newBookingStart },
                        { "endDate", newBookingEnd }
                    },
                    HumanizedDateTime = $"{newBookingStart:dddd d MMM, HH:mm tt} - {newBookingEnd:dddd d MMMM, HH:mm tt}"
                };
                bookingGaps.Add(bookingGapDto);
            }
        }

        return bookingGaps;
    } 

    public Task<BookingRefundDto> ComputeRefundForBooking(Booking booking)
    {
        var refundDto = ComputeRefundDto(booking);

        return Task.FromResult(refundDto);
    }

    public async Task<List<Booking>> GetBookingForProvider(Guid providerId)
    {
        var booking = await _context.Bookings.AsNoTracking().Where(b => b.ProviderId == providerId)
            .ToListAsync();
        return booking;
    }

    public Task<BookingHumanizedDto> GetHumanizedBooking(Booking booking)
    {
        return Task.FromResult(HumanizeBooking(booking));
    }

    public async Task<BookingAnalyticsDto> GetAnalyticsForAdmin()
    {
        var info = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        var currentTime = DateTime.UtcNow;
        var isDayLightSaving = info.IsDaylightSavingTime(currentTime);

        var bookings = await _context.Bookings.Where(b => b.BookingConfirmation == true)
            .Include(br => br.BookingRecord)
            .ToListAsync();

        var refundedBookings = await _context.Bookings
            .Where(b => b.BookingConfirmation == false && b.RefundStatus == true)
            .Include(br => br.BookingRecord)
            .ToListAsync();

        var noRefunds = await _context.Bookings.Where(b => b.BookingConfirmation == false && b.RefundStatus == false)
            .Include(br => br.BookingRecord)
            .ToListAsync();
        
        var currentDateTime = isDayLightSaving ? DateTime.UtcNow.AddHours(1) : DateTime.UtcNow;
        var startOfWeek = StartOfWeek(currentDateTime, DayOfWeek.Monday);
        
        // Explanations for the calculations are in the method below
        var endOfWeek = startOfWeek.AddDays(7).AddMilliseconds(-1);
        var startOfLast = startOfWeek.AddDays(-7);
        var endOfLast = startOfLast.AddDays(7).AddMilliseconds(-1);
        var dateYesterday = currentDateTime.AddDays(-1).Date;
        
        var bookingThisMonth = bookings
            .Where(b => b.CreatedAt.Date.Month == currentDateTime.Month).ToList();
        var bookingLastMonth = bookings
            .Where(b => b.CreatedAt.Month == currentDateTime.AddMonths(-1).Month).ToList();
        
        // fetch the bookings that occured last week or this week
        var bookingThisWeek = bookings
            .Where(b => b.CreatedAt.Date >= startOfWeek && b.CreatedAt <= endOfWeek).ToList();
        var bookingsLastWeek = bookings
            .Where(b => b.CreatedAt.Date >= startOfLast.Date && b.CreatedAt.Date <= endOfLast.Date).ToList();
        var todayBookings = bookings.Where(b => b.StartDate.Date == currentDateTime.Date).ToList();
        var yesterdayBookings = bookings.Where(b => b.StartDate.Date == dateYesterday.Date).ToList();
        
        // REVENUE FROM NO REFUNDS OR PARTIAL REFUNDS
        var refundLastWeek = refundedBookings
            .Where(b => b.CreatedAt.Date >= startOfLast.Date && b.CreatedAt.Date <= endOfLast.Date).ToList();
        
        var refundThisWeek = refundedBookings
            .Where(b => b.CreatedAt.Date >= startOfWeek && b.CreatedAt <= endOfWeek).ToList();
        
        var refundThisMonth = refundedBookings
            .Where(b => b.CreatedAt.Date.Month == currentDateTime.Month).ToList();
        
        var refundLastMonth = refundedBookings
            .Where(b => b.CreatedAt.Month == currentDateTime.AddMonths(-1).Month).ToList();
        
        var noRefundLastWeek = noRefunds
            .Where(b => b.CreatedAt.Date >= startOfLast.Date && b.CreatedAt.Date <= endOfLast.Date).ToList();
        
        var noRefundThisWeek = noRefunds
            .Where(b => b.CreatedAt.Date >= startOfWeek && b.CreatedAt <= endOfWeek).ToList();
        
        var noRefundThisMonth = noRefunds
            .Where(b => b.CreatedAt.Date.Month == currentDateTime.Month).ToList();
        
        var noRefundLastMonth = noRefunds
            .Where(b => b.CreatedAt.Month == currentDateTime.AddMonths(-1).Month).ToList();
        
        // CALCULATING THE TOTALS FOR THE REVENUES
        decimal totalLastWeek = bookingsLastWeek.Sum(b => b.BookingRecord.Fees)
                                + noRefundLastWeek.Sum(b => b.BookingRecord.Fees)
                                + refundLastWeek.Sum(b => b.Fees);

        decimal totalThisWeek = bookingThisWeek.Sum(b => b.BookingRecord.Fees)
                                + noRefundThisWeek.Sum(b => b.BookingRecord.Fees)
                                + refundThisWeek.Sum(b => b.Fees);
        
        // calculating totals for last month and current month
        decimal totalThisMonth = bookingThisMonth.Sum(b => b.BookingRecord.Fees)
                                 + noRefundThisMonth.Sum(b => b.BookingRecord.Fees)
                                 + refundLastWeek.Sum(b => b.Fees);

        decimal totalLastMonth = bookingLastMonth.Sum(b => b.BookingRecord.Fees)
                                 + noRefundLastMonth.Sum(b => b.BookingRecord.Fees)
                                 + refundLastMonth.Sum(b => b.Fees);
        
        // BUILDING THE DTO: BookingAnalyticsDto
        var bookingAnalyticsDto = new BookingAnalyticsDto
        {
            // temp Dto structure, no time to create dictionaries so gotta stick to this monstrosity for now
            // TODO: Convert this to a set of dictionaries to make it look better
            // foo { thisMonth: currentMonth: , currentWeek: , lastMonth: , lastWeek:, ... }
            BookingToday = todayBookings.Count,
            TotalBooking = bookings.Count,
            TotalBookingThisMonth = bookingThisMonth.Count,
            TotalBookingThisWeek = bookingThisWeek.Count,
            TotalBookingLastWeek = bookingsLastWeek.Count,
            TotalBookingLastMonth = bookingLastMonth.Count,
            TotalRefundsThisMonth = refundThisMonth.Count,
            BookingYesterday = yesterdayBookings.Count,
            BookingRevenueLastMonth = totalLastMonth.ToString("C", new CultureInfo("en-GB")),
            BookingRevenueCurrentWeek = totalThisWeek.ToString("C", new CultureInfo("en-GB")),
            BookingRevenueLastWeek = totalLastWeek.ToString("C", new CultureInfo("en-GB")),
            BookingRevenueThisMonth = totalThisMonth.ToString("C", new CultureInfo("en-GB")),
            ComparisonRevenueLastWeek = CreateComparisonString(totalLastWeek, totalThisWeek, "last week"),
            ComparisonBookingYesterday = CreateComparisonString(yesterdayBookings.Count, todayBookings.Count, "yesterday"),
            ComparisonBookingLastWeek = CreateComparisonString(bookingsLastWeek.Count, bookingThisWeek.Count, "last week"),
            ComparisonBookingLastMonth = CreateComparisonString(bookingLastMonth.Count, bookingThisMonth.Count, "last month"),
            ComparisonRevenueLastMonth = CreateComparisonString(totalLastMonth, totalThisMonth, "last month")
        };
        
        return bookingAnalyticsDto;
    }

    public async Task<BookingAnalyticsDto> GetAnalyticsForProviderById(Guid providerId)
    {
        var info = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        var currentTime = DateTime.UtcNow;
        var isDayLightSaving = info.IsDaylightSavingTime(currentTime);
        
        var bookings = await _context.Bookings.AsNoTracking()
            .Where(b => b.ProviderId == providerId && b.BookingConfirmation == true)
            .Include(booking => booking.BookingRecord)
            .ToListAsync();
        
        // Probably not the most performance-friendly approach but don't have time to do anything better... too bad!!
        var partialRefunds = await _context.Bookings
            .Where(b => b.ProviderId == providerId && b.BookingConfirmation == false && b.RefundStatus == true)
            .Include(booking => booking.BookingRecord)
            .ToListAsync();

        var noRefunds = await _context.Bookings
            .Where(b => b.ProviderId == providerId && b.BookingConfirmation == false && b.RefundStatus == false)
            .Include(br => br.BookingRecord)
            .ToListAsync();
        
        var activeFines = bookings.Where(b => b.FineStatus).ToList();

        var currentDateTime = isDayLightSaving ? DateTime.UtcNow.AddHours(1) : DateTime.UtcNow;
        var startOfWeek = StartOfWeek(currentDateTime, DayOfWeek.Monday);
        // We are adding 7 because the start of the week object is returned at 13/04/2024T00:00:00
        // when we add 6 days it will only add the days and not the time.
        // This means that booking on sunday after 12am won't be accounted for.
        var endOfWeek = startOfWeek.AddDays(7).AddMilliseconds(-1);
        var startOfLast = startOfWeek.AddDays(-7);
        var endOfLast = startOfLast.AddDays(7).AddMilliseconds(-1);
        var dateYesterday = currentDateTime.AddDays(-1).Date;
        
        var bookingThisMonth = bookings
            .Where(b => b.CreatedAt.Date.Month == currentDateTime.Month).ToList();
        var bookingLastMonth = bookings
            .Where(b => b.CreatedAt.Month == currentDateTime.AddMonths(-1).Month).ToList();
        
        // fetch the bookings that occured last week or this week
        var bookingThisWeek = bookings
            .Where(b => b.CreatedAt.Date >= startOfWeek && b.CreatedAt <= endOfWeek).ToList();
        var bookingsLastWeek = bookings
            .Where(b => b.CreatedAt.Date >= startOfLast.Date && b.CreatedAt.Date <= endOfLast.Date).ToList();
        var todayBookings = bookings.Where(b => b.StartDate.Date == currentDateTime.Date).ToList();
        var yesterdayBookings = bookings.Where(b => b.StartDate.Date == dateYesterday.Date).ToList();
        
        // REVENUE FROM NO REFUNDS OR PARTIAL REFUNDS
        var refundLastWeek = partialRefunds
            .Where(b => b.CreatedAt.Date >= startOfLast.Date && b.CreatedAt.Date <= endOfLast.Date).ToList();
        
        var refundThisWeek = partialRefunds
            .Where(b => b.CreatedAt.Date >= startOfWeek && b.CreatedAt <= endOfWeek).ToList();
        
        var refundThisMonth = partialRefunds
            .Where(b => b.CreatedAt.Date.Month == currentDateTime.Month).ToList();
        
        var refundLastMonth = partialRefunds
            .Where(b => b.CreatedAt.Month == currentDateTime.AddMonths(-1).Month).ToList();
        
        var noRefundLastWeek = noRefunds
            .Where(b => b.CreatedAt.Date >= startOfLast.Date && b.CreatedAt.Date <= endOfLast.Date).ToList();
        
        var noRefundThisWeek = noRefunds
            .Where(b => b.CreatedAt.Date >= startOfWeek && b.CreatedAt <= endOfWeek).ToList();
        
        var noRefundThisMonth = noRefunds
            .Where(b => b.CreatedAt.Date.Month == currentDateTime.Month).ToList();
        
        var noRefundLastMonth = noRefunds
            .Where(b => b.CreatedAt.Month == currentDateTime.AddMonths(-1).Month).ToList();
        
        // CALCULATING THE TOTALS FOR THE REVENUES
        decimal totalLastWeek = bookingsLastWeek.Sum(b => b.BookingRecord.SubTotal)
                                + noRefundLastWeek.Sum(b => b.BookingRecord.SubTotal)
                                + refundLastWeek.Sum(b => b.SubTotal - b.RefundAmount);

        decimal totalThisWeek = bookingThisWeek.Sum(b => b.BookingRecord.SubTotal)
                                + noRefundThisWeek.Sum(b => b.BookingRecord.SubTotal)
                                + refundThisWeek.Sum(b => b.SubTotal - b.RefundAmount);
        
        // calculating totals for last month and current month
        decimal totalThisMonth = bookingThisMonth.Sum(b => b.BookingRecord.SubTotal)
                                 + noRefundThisMonth.Sum(b => b.BookingRecord.SubTotal)
                                 + refundLastWeek.Sum(b => b.SubTotal - b.RefundAmount);

        decimal totalLastMonth = bookingLastMonth.Sum(b => b.BookingRecord.SubTotal)
                                 + noRefundLastMonth.Sum(b => b.BookingRecord.SubTotal)
                                 + refundLastMonth.Sum(b => b.SubTotal - b.RefundAmount);
        
        // BUILDING THE DTO: BookingAnalyticsDto
        var bookingAnalyticsDto = new BookingAnalyticsDto
        {
            // temp Dto structure, no time to create dictionaries so gotta stick to this monstrosity for now
            // TODO: Convert this to a set of dictionaries to make it look better
            // foo { thisMonth: currentMonth: , currentWeek: , lastMonth: , lastWeek:, ... }
            BookingToday = todayBookings.Count,
            TotalBooking = bookings.Count,
            TotalBookingThisMonth = bookingThisMonth.Count,
            TotalBookingThisWeek = bookingThisWeek.Count,
            TotalBookingLastWeek = bookingsLastWeek.Count,
            TotalBookingLastMonth = bookingLastMonth.Count,
            TotalRefundsThisMonth = refundThisMonth.Count,
            ActiveFines = activeFines.Count,
            BookingYesterday = yesterdayBookings.Count,
            BookingRevenueLastMonth = totalLastMonth.ToString("C", new CultureInfo("en-GB")),
            BookingRevenueCurrentWeek = totalThisWeek.ToString("C", new CultureInfo("en-GB")),
            BookingRevenueLastWeek = totalLastWeek.ToString("C", new CultureInfo("en-GB")),
            BookingRevenueThisMonth = totalThisMonth.ToString("C", new CultureInfo("en-GB")),
            ComparisonRevenueLastWeek = CreateComparisonString(totalLastWeek, totalThisWeek, "last week"),
            ComparisonBookingYesterday = CreateComparisonString(yesterdayBookings.Count, todayBookings.Count, "yesterday"),
            ComparisonBookingLastWeek = CreateComparisonString(bookingsLastWeek.Count, bookingThisWeek.Count, "last week"),
            ComparisonBookingLastMonth = CreateComparisonString(bookingLastMonth.Count, bookingThisMonth.Count, "last month"),
            ComparisonRevenueLastMonth = CreateComparisonString(totalLastMonth, totalThisMonth, "last month")
        };
        
        return bookingAnalyticsDto;
    }
    
    public Task<BookingPricing> GetPricingForPeriod(DateTime startDate, TimeSpan duration, decimal price)
    {
        // Related to the BookingPriceRequest. This will check the price of a booking for a given period.
        return Task.FromResult(CalculatePrice(startDate, duration, price));
    }

    public async Task<List<Booking>> GetBooking()
    {
        // AsNoTracking uses eager loading to improve performance. It loads it straight into memory and doesn't track changes
        var booking =  await _context.Bookings.AsNoTracking().ToListAsync();
        var sortedBooking = booking.OrderByDescending(b => b.BookingConfirmation)
            .ThenBy(b => b.StartDate)
            .ToList();

        return sortedBooking;
    }

    public async Task<List<Booking>> GetBookingForAccountId(Guid accountId)
    {
        // fetches bookings for specificId and then sorts them by bookingConfirmation and then DateTime
        var bookings = await _context.Bookings.AsNoTracking()
            .Where(b => b.AccountId == accountId)
            .ToListAsync();

        var bookingSortedList = BookingSortedList(bookings);

        return bookingSortedList;
    }

    public async Task<List<BookingHumanizedDto>> GetHumanizedBookingsByAccountId(Guid accountId)
    {
        // Rework of the function GetBookingForAccountId with some humanizer elements specific for cards
        var bookingList = await _context.Bookings.AsNoTracking()
            .Where(b => b.AccountId == accountId)
            .ToListAsync();
        
        var sortedBookingList = BookingSortedList(bookingList);

        var bookings = sortedBookingList.Select(booking =>
        {
            // create a new instance of HumanizedBooking object
            var humanizedBooking = HumanizeBooking(booking);
            return humanizedBooking;
        }).ToList();

        return bookings;
    }

    public async Task<Booking?> GetBooking(Guid id)
    {
        var booking = await _context.Bookings.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        return booking;
    }

    public async Task<Booking> CreateBooking(Booking booking, decimal price)
    {
        var info = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        
        var isDayLightSavingCreatedAt = info.IsDaylightSavingTime(booking.CreatedAt);

        if (isDayLightSavingCreatedAt)
        {
            booking.CreatedAt = DateTime.UtcNow.AddHours(1);
            booking.LastUpdated = DateTime.UtcNow.AddHours(1);
        }
        
        var generatedId = Guid.NewGuid();
        booking.Id = generatedId;
        // Didn't like using generated defaults, too bad!!
        booking.BookingConfirmation = true;

        var bookingDuration = booking.EndDate - booking.StartDate;
        var bookingPriceCalc = CalculatePrice(booking.StartDate, bookingDuration, price);
        
        // map the pricing of the parking spot to the booking
        _mapper.Map(bookingPriceCalc, booking);
        
        booking.CarRegistration = booking.CarRegistration.ToUpper();

        var bookingRecord = await _context.BookingRecords
            .FirstOrDefaultAsync(p => p.Id == booking.RecordId);
        
        // terrible and risky way of doing this, but it should always in theory be unassigned.
        // this will cause a crash if another TransactionId is assigned already...
        bookingRecord!.Verified = true;
        
        await _context.Bookings.AddAsync(booking);
        
        await _context.SaveChangesAsync();
        
        return booking;
    }

    public async Task<bool> UpdateBooking(BookingUpdateDto booking)
    {
        var existingBooking = await _context.Bookings
            .Include(b => b.BookingRecord)
            .FirstOrDefaultAsync
            (p => p.Id == booking.Id);

        if (existingBooking == null) return false;
        
        // map objects to the existing booking from the booking
        _mapper.Map(booking, existingBooking);

        var info = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        var currentTime = DateTime.UtcNow;
        var isDayLightSaving = info.IsDaylightSavingTime(currentTime);

        existingBooking.LastUpdated = isDayLightSaving ? DateTime.UtcNow.AddHours(1) : DateTime.UtcNow;
        existingBooking.CarRegistration = booking.CarRegistration.ToUpper();
        
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> SubmitRefund(BookingUpdateDto booking)
    {
        var newUpdateDto = new BookingUpdateDto();

        var existingBooking = await _context.Bookings
            .Include(b => b.BookingRecord)
            .FirstOrDefaultAsync
                (p => p.Id == booking.Id);

        if (existingBooking == null) return false;

        // when the booking is cancelled, the booking record alongside the BookingRecord is unverified
        existingBooking.BookingRecord.Verified = false;
        
        var bookingRefund = ComputeRefundDto(existingBooking);
        
        if (bookingRefund.NoRefund == false)
        {
            var bookingRefundObj = new BookingRefund
            {
                AccountId = existingBooking.AccountId,
                BookingId = existingBooking.Id,
                ProviderId = existingBooking.ProviderId,
                RecordId = existingBooking.RecordId,
                Total = bookingRefund.TotalRefund
            };
            await _context.BookingRefunds.AddAsync(bookingRefundObj);
            existingBooking.RefundStatus = true;
            existingBooking.RefundAmount = bookingRefund.TotalRefund;
            
            // send notification email that the refund has been processed.
            
            // new little hack that... due to the way Newtonsoft handles serialization, 
            // the foreign key turns into a circular reference and messes stuff up.
            // must map to an update dto when sending emails lol.
            // this hack occurs anywhere in the code where we send emails.
            _mapper.Map(existingBooking, newUpdateDto);
            await _notificationService.SendBookingRefundEmail(bookingRefundObj, newUpdateDto, existingBooking.Email);
        }
        else
        {
            // send an alternative email that the refund has not been processed.
            await _notificationService.SendNoRefundEmail(existingBooking.Email, booking.Id.ToString());
        }
        
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteBooking(Guid id)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(p => p.Id == id);

        if (booking == null) return false;
        
        _context.Bookings.Remove(booking);
        await _context.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<List<Booking>> GetFutureBookingForParkingId(Guid parkingId)
    {
        // This is used to find bookings that are currently booked now or in the future
        var info = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        var isDayLightSaving = info.IsDaylightSavingTime(DateTime.UtcNow);
        
        var bookings = await _context.Bookings.AsNoTracking()
            .Where(b => b.ParkingId == parkingId && b.StartDate >= 
                (isDayLightSaving ? DateTime.UtcNow.AddHours(1) : DateTime.UtcNow)).ToListAsync();
        
        return bookings;
    }

    public async Task<bool> CheckSpotAvailableGrpc(Guid parkingId, DateTime dateTime, TimeSpan duration, int slotCapacity)
    {
        // This is primarily used by a gRPC request made by the consumer in the parking api.
        // This is used to recommend parking spot availability
        // it's essentially the same as the one below,
        // but it has 3 parameters which is the only thing that the gRPC from consumer consumes.
        // It's a bit ugly, but I can't find a better way to do it really
        var newStart = dateTime;
        var newEnd = dateTime.Add(duration);
        
        var overlappingBookings = await _context.Bookings
            .AsNoTracking()
            .Where(b =>
                b.BookingConfirmation == true &&
                b.ParkingId == parkingId &&
                b.EndDate > newStart && b.StartDate < newEnd) // Check for overlapping dates
            .ToListAsync();
        
        // these bookings span a days over the current bookings

        var sameDayBookings = await _context.Bookings
            .AsNoTracking()
            .Where(p => (p.StartDate.Date.Equals(newStart.Date) || p.EndDate.Date.Equals(newStart.Date)) && 
                        p.BookingConfirmation == true && p.ParkingId == parkingId)
            .ToListAsync();
        
        var sameDayBookingIds = sameDayBookings.Select(p => p.Id).ToList();
        var overlappedRemovals = overlappingBookings.Where(ob => sameDayBookingIds.Contains(ob.Id)).ToList();
        
        // remove the overlapped bookings to make sure we can check to see if there is a free slot
        foreach (var bookingToRemove in overlappedRemovals)
        {
            overlappingBookings.Remove(bookingToRemove);
        }
        
        // do a quick check to see if we can simply do an early return if the space is at capacity
        if (overlappingBookings.Count >= slotCapacity) return false;

        slotCapacity -= overlappingBookings.Count;
        
        return await Task.FromResult(CheckBooking(sameDayBookings, newStart, newEnd, duration, slotCapacity));
    }

    public async Task<bool> CheckIfSpotAvailable(Booking booking, int capacity)
    {
        var newStart = booking.StartDate;
        var newEnd = booking.EndDate;
        
        // these bookings span days over the current bookings
        var overlappingBookings = await _context.Bookings
            .AsNoTracking()
            .Where(b =>
                b.BookingConfirmation == true &&
                b.ParkingId == booking.ParkingId &&
                booking.Id != b.Id &&
                b.EndDate > newStart && b.StartDate < newEnd) // Check for overlapping dates
            .ToListAsync();
        
        var sameDayBookings = await _context.Bookings
            .AsNoTracking()
            .Where(p => (p.StartDate.Date.Equals(newStart.Date) || p.EndDate.Date.Equals(newStart.Date)) && 
                        p.BookingConfirmation == true && p.ParkingId == booking.ParkingId)
            .ToListAsync();
        
        // add booking to list that appear overlapped but are on the same day
        var sameDayBookingIds = sameDayBookings.Select(p => p.Id).ToList();
        var overlappedRemovals = overlappingBookings.Where(ob => sameDayBookingIds.Contains(ob.Id)).ToList();
        
        // remove the overlapped bookings to make sure we can check to see if there is a free slot
        foreach (var bookingToRemove in overlappedRemovals)
        {
            overlappingBookings.Remove(bookingToRemove);
        }
        
        // do a quick check to see if we can simply do an early return if the space is at capacity
        if (overlappingBookings.Count >= capacity) return false;

        capacity -= overlappingBookings.Count;
        
        var bookingDuration = booking.EndDate - booking.StartDate;
        // optimized loop using span to increase performance (reference - https://www.youtube.com/watch?v=cwBrWn4m9y8)
        return await Task.FromResult(CheckBooking(sameDayBookings, newStart, newEnd, bookingDuration, capacity));
    }
    
    public async Task<BookingRefund> UpdateBookingParkingDeleted(Guid bookingId)
    {
        var existingBooking = await _context.Bookings
            .Include(b => b.BookingRecord)
            .FirstOrDefaultAsync
                (p => p.Id == bookingId);

        if (existingBooking == null) return null!;
        
        var info = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        var currentTime = DateTime.UtcNow;
        var isDayLightSaving = info.IsDaylightSavingTime(currentTime);

        existingBooking.LastUpdated = isDayLightSaving ? DateTime.UtcNow.AddHours(1) : DateTime.UtcNow;
        existingBooking.RefundAmount = existingBooking.SubTotal;
        existingBooking.RefundStatus = true;
        existingBooking.BookingConfirmation = false;
        
        var bookingRefundObj = new BookingRefund
        {
            AccountId = existingBooking.AccountId,
            BookingId = existingBooking.Id,
            ProviderId = existingBooking.ProviderId,
            RecordId = existingBooking.RecordId,
            Total = existingBooking.RefundAmount
        };
        
        await _context.BookingRefunds.AddAsync(bookingRefundObj);

        await _context.SaveChangesAsync();

        return bookingRefundObj;
    }
    
    // --------------------- END BOOKING REPOSITORIES -------------------------
    
    
    
    // --------------------- LOCAL PRIVATE FUNCTIONS ---------------------------
    
    private bool CheckBooking(List<Booking> sameDayBookings, DateTime newStart, DateTime newEnd, TimeSpan duration, int capacity)
    {
        Span<Booking> itemSpan = CollectionsMarshal.AsSpan(sameDayBookings);
        ref var searchSpace = ref MemoryMarshal.GetReference(itemSpan);
        
        // using the TimeRange library to create a new TimeRange which we will 
        // compare against existing bookings.
        ITimePeriod newBooking = new TimeRange
        {
            Start = newStart,
            Duration = duration,
            End = newEnd
        };

        int slotCount = 0;
        
        for (var i = 0; i < itemSpan.Length; i++)
        {
            var item = Unsafe.Add(ref searchSpace, i);
            var existingStart = item.StartDate;
            var existingEnd = item.EndDate;
            
            // instantiating a new instance of TimeRange for an existing booking
            ITimePeriod existingBooking = new TimeRange
            {
                Start = existingStart,
                End = existingEnd
            };
            
            // overlaps only checks for overlapping periods. the periods are allowed to touch
            var overlap = newBooking.OverlapsWith(existingBooking);
            
            // if there is an overlap increment
            if (overlap)
            {
                slotCount++;
            }
            
            // check to see if the slotCount is at capacity
            if (slotCount >= capacity)
            {
                return false;
            }
        }
        return true;
    }
    
    private BookingPricing CalculatePrice(DateTime bookingDate, TimeSpan bookingDuration, decimal parkingPrice)
    {
        const decimal transactionFeePercentage = 5M; // 5% transaction fee (adjust as needed)
        const decimal processingFeePercentage = 25M; // 25% processing fee (adjust as needed)
        
        var bookingInterval = bookingDuration.TotalMinutes / 30;

        var subTotal = 0M;
        
        for (int i = 0; i < bookingInterval; i++)
        {
            DateTime bookingStart = bookingDate.AddMinutes(i * 30);

            decimal hourlyRate = (decimal)30 / 60 * parkingPrice; // Calculate price for the current interval

            if (bookingStart.Hour is >= 7 and < 14)
            {
                subTotal += hourlyRate; // Normal rate
            }
            else if (bookingStart.Hour is >= 14 and < 17)
            {
                subTotal += hourlyRate * 1.75M; // Rush hour rate (adjust as needed)
            }
            else if (bookingStart.DayOfWeek is DayOfWeek.Friday or DayOfWeek.Saturday
                     && bookingStart.Hour is >= 17 and < 21)
            {
                subTotal += hourlyRate * 1.35M;
            }
            else if (bookingStart is { DayOfWeek: DayOfWeek.Sunday, Hour: >= 17 and < 21 })
            {
                subTotal += hourlyRate * 1.15M;
            }
            else if (bookingStart.Hour is >= 17 and < 21)
            {
                subTotal += hourlyRate * 1.25M;
            }
            else
            {
                subTotal += hourlyRate * 0.5M; // Quiet period rate (adjust as needed)
            }
        }

        var transactionFee = subTotal * (transactionFeePercentage / 100);
        var processingFee = subTotal * (processingFeePercentage / 100);

        const decimal discountPerDay = 0.85M;

        var days = (int)Math.Ceiling(bookingDuration.TotalHours / 24);
        var discountAmount = 0M;
        
        for (int i = 0; i < days; i++)
        {
            discountAmount += subTotal * (1 - discountPerDay);
        }

        subTotal -= discountAmount;

        var fees = transactionFee + processingFee;

        var totalCost = subTotal + fees;

        var bookingPricing = new BookingPricing()
        {
            Fees = Math.Round(fees, 2),
            Total = Math.Round(totalCost, 2),
            SubTotal = Math.Round(subTotal, 2)
        };

        return bookingPricing;
    }
    
    private BookingHumanizedDto HumanizeBooking(Booking booking)
    {
        var info = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        var createdAtDateTime = booking.CreatedAt;
        var isDayLightSaving = info.IsDaylightSavingTime(createdAtDateTime);
        
        // basically humanizer only works with UTC, which means that
        // I have to take away 1 hour from the createdAt date to get accurate results
        // e.g., Users will be in the future "59 minutes from now" if they just booked a slot

        var bookingDuration = booking.EndDate - booking.StartDate;
        
        var humanizedDto = new BookingHumanizedDto
        {
            HumanizedTotal = booking.Total.ToString("C", new CultureInfo("en-GB")),
            HumanizedFees = booking.Fees.ToString("C", new CultureInfo("en-GB")),
            HumanizedSubTotal = booking.SubTotal.ToString("C", new CultureInfo("en-GB")),
            HumanizedDuration = bookingDuration.Humanize(),
            HumanizedCreatedAt = isDayLightSaving ? 
                // hack for humanizer incoming!!! =) ....
                booking.CreatedAt.AddHours(-1).Humanize() : 
                booking.CreatedAt.Humanize(),
            HumanizedCreatedAtDate = booking.CreatedAt.ToString("dddd d MMM, HH:mm tt"),
            HumanizedDate = booking.StartDate.ToString("dddd d MMM, HH:mm tt"),
            HumanizedBookingSpan = booking.StartDate.ToString("dddd d MMM, HH:mm tt") + " - " +
                                   booking.EndDate.ToString("dddd d MMM, HH:mm tt"),
            HumanizedLastUpdated = booking.LastUpdated.ToString("dddd d MMM, HH:mm tt"),
            HumanizedRefundAmount = booking.RefundAmount.ToString("C", new CultureInfo("en-GB"))
        };

        // map the remaining values from booking to the HumanizedBooking object
        _mapper.Map(booking, humanizedDto);
        
        return humanizedDto;
    }
    
    private DateTime StartOfWeek(DateTime dt, DayOfWeek startOfWeek)
    {
        int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
        return dt.AddDays(-1 * diff).Date;
    }

    private string CreateComparisonString(decimal past, decimal present, string period)
    {
        /*
         * period - is the prefix/timespan being calculated
         * past - is the period in the past that is being compared
         * present - is the current period that is being compared
         */
        var comparisonResult = "Not enough data to make comparison";
    
        if (past <= 0) return comparisonResult;
    
        decimal percentageDifference = (present - past) / past * 100;
        comparisonResult = percentageDifference.ToString("0.00") + "% ";

        comparisonResult += percentageDifference switch
        {
            > 0 => "increase from " + period,
            < 0 => "decrease from " + period,
            _ => "no change from " + period
        };

        return comparisonResult;
    }
    
    private List<Booking> BookingSortedList(List<Booking> bookings)
    {
        // Sorting bookings by booking confirmation and then datetime.
        // This makes sure that the booking that is confirmed and latest is at the top.
        var info = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        var isDayLightSaving = info.IsDaylightSavingTime(DateTime.UtcNow);
        var currentTime = isDayLightSaving ? DateTime.UtcNow.AddHours(1) : DateTime.UtcNow;
        
        // bookings are ordered descending by future bookings and then the closest to the current time
        var finalSortedList = bookings.Where(b => b.StartDate >= currentTime && b.BookingConfirmation)
            .OrderBy(b => (b.StartDate - currentTime).Duration())
            .ToList();
        
        // get bookings in the past in the same fashion
        var pastBookings = bookings.Where(b => b.StartDate < currentTime && b.BookingConfirmation)
            .OrderBy(b => (b.StartDate - currentTime).Duration())
            .ToList();
        
        finalSortedList.AddRange(pastBookings);
        
        // get canceled bookings in the same fashion
        finalSortedList.AddRange(bookings.Where(b => b.BookingConfirmation == false));

        return finalSortedList;
    }
    
    private BookingRefundDto ComputeRefundDto(Booking booking)
    {
        DateTime bookingDate = booking.StartDate;
        
        var info = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        var currentTime = DateTime.UtcNow;
        var isDayLightSaving = info.IsDaylightSavingTime(currentTime);
        var currentDateTime = isDayLightSaving ? DateTime.UtcNow.AddHours(1) : DateTime.UtcNow;
        
        var timeDifference = bookingDate - currentDateTime;

        decimal refundPercentage;
        var fullRefund = true;
        var noRefund = false;

        switch (timeDifference.TotalHours)
        {
            case <= 4:
                refundPercentage = 0;
                fullRefund = false;
                noRefund = true;
                break;
            case <= 24:
                refundPercentage = 0.5m; // 50% refund within 24 hours
                fullRefund = false;
                break;
            case > 24 and <= 48:
                refundPercentage = 0.75m; // 75% refund between 24 and 48 hours
                fullRefund = false;
                break;
            default:
                refundPercentage = 1; // Full refund after 48 hours
                break;
        }

        // Calculate refund amount - the processing fees are not included in the refund
        var refundAmount = booking.SubTotal * refundPercentage;

        // Create and return the BookingRefundDto
        var refundDto = new BookingRefundDto
        {
            TotalRefund = Math.Round(refundAmount + 0.005m, 2),
            BookingId = booking.Id,
            NoRefund = noRefund,
            RefundAmount = Math.Round(refundAmount + 0.005m, 2)
                .ToString("C", new CultureInfo("en-GB")),
            // when working with rounding, if the number is 0.5, it will round down
            // adding 0.005 makes sure that it rounds up.
            RefundPercentage = (refundPercentage * 100).ToString("0.00") + "%",
            FullRefund = fullRefund
        };

        var humanizedBooking = HumanizeBooking(booking);
    
        // map some of the humanized fields to the refundDto
        _mapper.Map(humanizedBooking, refundDto);
        return refundDto;
    }
}