namespace PersonalAssistant.Api.Features.Chat.SendMessage;

using Microsoft.EntityFrameworkCore;

using PersonalAssistant.Api.Domain.Enums;
using PersonalAssistant.Api.Domain.Entities;

using PersonalAssistant.Api.Infrastructure.Llm;
using PersonalAssistant.Api.Infrastructure.Database;

using PersonalAssistant.Api.Common.Contracts.AiService;

public class SendMessageHandler(AppDbContext db, ILlmClient llmClient)
{

    public async Task<SendMessageResponse> HandleAsync(
        SendMessageRequest request,
        CancellationToken ct = default)
    {
        // 1. Obtener o crear la sesión de chat
        var chatSession = await GetOrCreateSessionAsync(request.SessionId, request.Message, ct);

        // 2. Registrar el mensaje del usuario
        var userMessage = new ChatMessage
        {
            SessionId = chatSession.Id,
            Role = MessageRole.User,
            Content = request.Message.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        db.ChatMessages.Add(userMessage);
        await db.SaveChangesAsync(ct);

        // 3) Leer historial reciente del chat
        var recentHistory = await db.ChatMessages
            .AsNoTracking()
            .Where(m => m.SessionId == chatSession.Id && m.Id != userMessage.Id)
            .Take(8)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new ChatMessageItem(m.Role, m.Content))
            .ToListAsync(ct);

        // crea flag de resumen
        int totalMessagesCount = await db.ChatMessages
            .CountAsync(m => m.SessionId == chatSession.Id, ct);
        bool shouldSummarize = chatSession.Messages.Count >= 8 && (totalMessagesCount % 8 == 0);

        // 4) Crear el request para el servicio de IA
        var aiRequest = new AiServiceRequest(
            SessionId: chatSession.Id,
            UserMessage: userMessage.Content,
            History: recentHistory,
            CurrentSumary: chatSession.Summary,
            Task: "chat",
            Persona: "assistant personal",
            Tone: "concise",
            ShouldSummarize: shouldSummarize
        );

        // 5) Llamar al servicio de IA
        var aiReply = await llmClient.GenerateResponseAsync(aiRequest, ct);

        // 6) Guardar la respuesta del asistente
        var assistantMessage = new ChatMessage
        {
            SessionId = chatSession.Id,
            Role = MessageRole.Assistant,
            Content = aiReply,
            CreatedAt = DateTime.UtcNow
        };
        db.ChatMessages.Add(assistantMessage);
        await db.SaveChangesAsync(ct);

        // 7) Devolver la respuesta
        return new SendMessageResponse(
            Id: assistantMessage.Id,
            SessionId: assistantMessage.SessionId,
            Role: assistantMessage.Role,
            Content: assistantMessage.Content,
            CreatedAt: assistantMessage.CreatedAt
        );
    }

    private async Task<ChatSession> GetOrCreateSessionAsync(
        Guid? sessionId,
        string firstMessage,
        CancellationToken ct)
    {
        // Si el cliente envió un ID, buscamos si ya existe en la BD
        if (sessionId.HasValue)
        {
            var existingSession = await db.ChatSessions.FindAsync([sessionId.Value], ct);
            if (existingSession is not null)
            {
                return existingSession;
            }
        }

        // Si sessionId era null o no existía, creamos una nueva sesión
        var newSession = new ChatSession
        {
            Id = sessionId ?? Guid.NewGuid(),
            Title = firstMessage.Length > 50
                ? $"{firstMessage[..47]}..."
                : firstMessage,
            CreatedAt = DateTime.UtcNow
        };

        db.ChatSessions.Add(newSession);
        return newSession;
    }
}