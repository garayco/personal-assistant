namespace PersonalAssistant.Api.Features.Chat.SendMessage;

using PersonalAssistant.Api.Domain.Enums;
using PersonalAssistant.Api.Domain.Entities;
using PersonalAssistant.Api.Infrastructure.Database;
using PersonalAssistant.Api.Infrastructure.Llm;


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

        // Se guarda en PostgreSQL: si el LLM falla, el prompt del usuario no se pierde
        await db.SaveChangesAsync(ct);

        // 3. Ejecutar inferencia con el modelo de lenguaje (LLM)
        string aiReply = await llmClient.GenerateResponseAsync(request.Message, ct);

        // 4. Registrar la respuesta del asistente
        var assistantMessage = new ChatMessage
        {
            SessionId = chatSession.Id,
            Role = MessageRole.Assistant,
            Content = aiReply,
            CreatedAt = DateTime.UtcNow
        };
        db.ChatMessages.Add(assistantMessage);
        await db.SaveChangesAsync(ct);

        // 5. Retornar DTO de respuesta
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