namespace LegalMind.Web.Domain.Entities;

public class ChatMessage
{
    public long Id { get; set; }
    public long ThreadId { get; set; }
    public ChatThread Thread { get; set; } = null!;

    public string Role { get; set; } = "user"; // user/assistant/system
    public string Content { get; set; } = "";
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
