namespace Booking_Domain.Data;

public class BookingAnalyticsDto
{
    // TODO: No time to make this look better so we will have to go with this monstrosity for now...
    // if there's time, use dictionaries to fix this shit
    
    // Number of bookings today
    public int BookingToday { get; set; }
    // Number of bookings yesterday
    public int BookingYesterday { get; set; }
    // Current active fines
    public int ActiveFines { get; set; }
    // Total number of bookings this month
    public int TotalBookingThisMonth { get; set; }
    // Total number of bookings previous month
    public int TotalBookingLastMonth { get; set; }
    // The overall total of bookings
    public int TotalBooking { get; set; }
    // Number of bookings this week
    public int TotalBookingThisWeek { get; set; }
    // Number of bookings last week
    public int TotalBookingLastWeek { get; set; }
    // Total refunds this month
    public int TotalRefundsThisMonth { get; set; }
    // Revenue last week
    public string BookingRevenueLastWeek { get; set; }
    // Revenue this month
    public string BookingRevenueThisMonth { get; set; }
    // Revenue last month
    public string BookingRevenueLastMonth { get; set; }
    // Revenue current week
    public string BookingRevenueCurrentWeek { get; set; }
    // comparison between last month and this month revenue wise
    public string ComparisonRevenueLastMonth { get; set; }
    // comparison between last week and this week revenue wise
    public string ComparisonRevenueLastWeek { get; set; }
    // comparison between last week and this week booking wise
    public string ComparisonBookingLastWeek { get; set; }
    // comparison between bookings today and yesterday
    public string ComparisonBookingYesterday { get; set; }
    // comparison between bookings this month and last month
    public string ComparisonBookingLastMonth { get; set; }
}