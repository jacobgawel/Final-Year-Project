using System.Text.Json.Serialization;

namespace Booking_Domain.Entities;

public class BookingRecord
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string CardNumber { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal Total { get; set; }
    public decimal Fees { get; set; }
    public decimal SubTotal { get; set; }
    public bool Verified { get; set; } // this state determines whether the booking transaction has been
    // successful or not
    public bool DepositStatus { get; set; }
    
    [JsonIgnore]
    public Booking Booking { get; set; }
}