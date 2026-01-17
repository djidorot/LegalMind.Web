namespace LegalMind.Web.Services.AI.Models;

public class LegalAnswer
{
    public string Summary { get; set; } = "";
    public string Guidance { get; set; } = "";
    public string Confidence { get; set; } = "Low"; // Low/Medium/High
    public string Risk { get; set; } = "Low";       // Low/Moderate/High
    public List<CitedSource> Citations { get; set; } = new();
    public List<string> NextSteps { get; set; } = new();
    public string Disclaimer { get; set; } =
        "This is for informational and educational purposes only and is not legal advice. Consult a licensed attorney for advice about your specific situation.";
}

public class CitedSource
{
    public string Title { get; set; } = "";
    public string Citation { get; set; } = "";
    public string? Url { get; set; }
    public DateTime? LastUpdatedUtc { get; set; }
}
