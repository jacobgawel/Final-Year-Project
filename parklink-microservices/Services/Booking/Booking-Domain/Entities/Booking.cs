using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Booking_Domain.Entities;

[Table("Bookings")]
public class Booking
{
    public Guid Id { get; set; }
    public string CarRegistration { get; set; }
    public Guid AccountId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool FineStatus { get; set; }
    public bool FinePaid { get; set; }
    public string Email { get; set; }
    public Guid ProviderId { get; set; }
    public bool BookingConfirmation { get; set; }
    public Guid ParkingId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public decimal SubTotal { get; set; }
    public decimal Fees { get; set; }
    public decimal Total { get; set; }
    public string? QrCodeLink { get; set; }
    public DateTime LastUpdated { get; set; }
    public bool RefundStatus { get; set; }
    public decimal RefundAmount { get; set; }
    
    // ---------- foreign keys -------------
    public Guid RecordId { get; set; }
    [JsonIgnore]
    public BookingRecord BookingRecord { get; set; }
}