namespace LegalMind.Web.Models;

public class DashboardViewModel
{
    public string? DisplayName { get; set; }
    public List<DashboardThreadItem> RecentThreads { get; set; } = new();
}

public class DashboardThreadItem
{
    public long ThreadId { get; set; }
    public string? Title { get; set; }
    public string Jurisdiction { get; set; } = "";
    public DateTime CreatedUtc { get; set; }
    public int MessageCount { get; set; }
}
