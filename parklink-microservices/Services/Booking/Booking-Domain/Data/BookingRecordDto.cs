namespace Booking_Domain.Data;

public class BookingRecordDto
{
    // request object that the client sends 
    // to the backend to create a BookingRecord
    // these details then get mapped automatically to the BookingRecord
    // object which is then saved to the database.
    public string CardNumber { get; set; }
    public decimal Total { get; set; }
    public decimal Fees { get; set; }
    public decimal SubTotal { get; set; }
}