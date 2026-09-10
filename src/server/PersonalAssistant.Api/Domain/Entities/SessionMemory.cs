namespace PersonalAssistant.Api.Domain.Entities;

using Pgvector;

public class SessionMemory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChatSessionId { get; set; }
    public string Content { get; set; } = string.Empty;
    public Vector Embedding { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ChatSession ChatSession { get; set; } = null!;
}
