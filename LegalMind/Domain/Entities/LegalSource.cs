namespace LegalMind.Web.Domain.Entities;

public class LegalSource
{
    public int Id { get; set; }

    public string Title { get; set; } = "";
    public string Jurisdiction { get; set; } = "";   // e.g., "PH-NCR" or "US-CA"
    public string SourceType { get; set; } = "Statute"; // Statute/Regulation/CaseLaw/Guidance
    public string? Citation { get; set; }            // e.g., "RA 11232, Sec. 5"
    public string? Url { get; set; }
    public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;

    public string Status { get; set; } = "Verified"; // Draft/Verified/Deprecated
}
