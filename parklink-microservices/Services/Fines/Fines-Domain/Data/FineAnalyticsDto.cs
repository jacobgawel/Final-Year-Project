namespace Fines_Domain.Data;

public class FineAnalyticsDto
{
    public int ActiveFines { get; set; }
    public int PendingFines { get; set; }
    public int TotalFines { get; set; }
    public int PaidFines { get; set; }
    public string FineRevenue { get; set; }
}