namespace Booking_Domain.Topics;

public class BookingRecommend
{
    // EVENT describes the event that is being created
    // used to make it more explicit as to what is happening in the debugging
    // The booking list contains the list of events that are required to contain recommendations
    // e.g. the bookings that are conflicted
    
    public string Event { get; set; }
    public List<BookingConflict> Bookings { get; set; }
}