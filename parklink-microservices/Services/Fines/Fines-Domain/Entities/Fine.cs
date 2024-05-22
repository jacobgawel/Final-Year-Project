using System.ComponentModel.DataAnnotations.Schema;

namespace Fines_Domain.Entities;

[Table("Fines")]
public class Fine
{
    public Guid Id { get; set; }
    public string Description { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid FineIssuerId { get; set; } // the fine issuer id is related to the provider/admin who issued the ticket
    public Guid BookingId { get; set; } // the booking id the fine belongs to
    public Guid AccountId { get; set; } // the account id of the user who has the fine
    public bool FineStatus { get; set; } // admin approval is required by default when the fine is created
    public bool FinePaid { get; set; }
    public string ImageUri { get; set; }
}