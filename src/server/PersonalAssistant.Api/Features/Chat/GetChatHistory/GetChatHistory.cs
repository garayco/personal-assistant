namespace PersonalAssistant.Api.Features.Chat.GetChatHistory;

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PersonalAssistant.Api.Domain.Enums;
using PersonalAssistant.Api.Infrastructure.Database;

public record ChatHistoryItem(
    Guid Id,
    Guid SessionId,
    MessageRole Role,
    string Content,
    DateTime CreatedAt
);

public record GetChatHistoryResponse(
    Guid SessionId,
    int TotalMessages,
    IReadOnlyList<ChatHistoryItem> Messages
);

public class GetChatHistoryHandler(AppDbContext db)
{

    public async Task<GetChatHistoryResponse?> HandleAsync(Guid sessionId, CancellationToken ct)
    {
        var sessionExists = await db.ChatSessions.AnyAsync(s => s.Id == sessionId, ct);
        if (!sessionExists)
        {
            return null; // La sesión no existe
        }

        var messages = await db.ChatMessages
            .AsNoTracking()
            .Where(m => m.SessionId == sessionId)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new ChatHistoryItem(
                m.Id,
                m.SessionId,
                m.Role,
                m.Content,
                m.CreatedAt
            ))
            .ToListAsync(ct);

        return new GetChatHistoryResponse(sessionId, messages.Count, messages);
    }
}