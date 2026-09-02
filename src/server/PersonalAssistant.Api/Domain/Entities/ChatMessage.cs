namespace PersonalAssistant.Api.Domain.Entities;

using PersonalAssistant.Api.Domain.Enums;

public class ChatMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public MessageRole Role { get; set; } = MessageRole.User;

    //FK
    public Guid ChatSessionId { get; set; }
    public ChatSession ChatSession { get; set; } = null!;
}