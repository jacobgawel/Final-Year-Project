namespace Booking_Domain.Topics;

public class BookingConflict
{
    // This is sent to the parking spot recommendation consumer
    // this format provides all the necessary details about a booking that the
    // parking consumer will need to find a recommended booking
    public string Email {get;set;}
    public DateTime BookingDate {get;set;}
    public TimeSpan Duration {get;set;}
    public string City {get;set;}
    public double? Longitude {get; set;}
    public double? Latitude { get; set; }
}