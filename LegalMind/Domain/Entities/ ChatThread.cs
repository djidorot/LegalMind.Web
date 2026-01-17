namespace LegalMind.Web.Domain.Entities;

public class ChatThread
{
    public long Id { get; set; }
    public string UserId { get; set; } = "";
    public string Title { get; set; } = "New question";
    public string Jurisdiction { get; set; } = "PH";
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public List<ChatMessage> Messages { get; set; } = new();
}
