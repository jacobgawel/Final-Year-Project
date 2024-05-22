namespace Fines_Domain.Data;

public class FineUpdateDto
{
    public Guid Id { get; set; }
    public string Description { get; set; }
    public string ImageUri { get; set; }
    public bool FineStatus { get; set; }
    public bool FinePaid { get; set; }
}