namespace PersonalAssistant.Api.Features.Chat;

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Pgvector.EntityFrameworkCore;

using PersonalAssistant.Api.Common.Contracts;
using PersonalAssistant.Api.Domain.Entities;
using PersonalAssistant.Api.Domain.Enums;
using PersonalAssistant.Api.Infrastructure.Database;
using PersonalAssistant.Api.Infrastructure.Llm;
using PersonalAssistant.Api.Infrastructure.Summaries;

public record SendMessageRequest(
    Guid? SessionId,
    string Message
);

public record SendMessageResponse(
    Guid Id,
    Guid SessionId,
    MessageRole Role,
    string Content,
    DateTime CreatedAt
);

public class SendMessageValidator : AbstractValidator<SendMessageRequest>
{
    public SendMessageValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("El mensaje no puede estar vacío.")
            .MaximumLength(4000)
            .WithMessage("El mensaje no puede superar los 4000 caracteres.");
    }
}

public class SendMessageHandler(
    AppDbContext db,
    ILlmClient llmClient,
    ISummaryQueue summaryQueue,
    IOptions<SummaryOptions> summaryOptions,
    ILogger<SendMessageHandler> logger)
{
    private readonly SummaryOptions _options = summaryOptions.Value;

    public async Task<SendMessageResponse> HandleAsync(
        SendMessageRequest request,
        CancellationToken ct = default)
    {
        var chatSession = await GetOrCreateSessionAsync(request.SessionId, request.Message, ct);

        var userMessage = new ChatMessage
        {
            ChatSessionId = chatSession.Id,
            Role = MessageRole.User,
            Content = request.Message.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        db.ChatMessages.Add(userMessage);
        await db.SaveChangesAsync(ct);

        // 1. Memoria semántica a largo plazo (RAG con pgvector)
        var messageEmbedding = await llmClient.GetEmbeddingAsync(userMessage.Content, ct);
        var retrievedFacts = await db.SessionMemories
            .AsNoTracking()
            .OrderBy(m => m.Embedding.CosineDistance(messageEmbedding))
            .Take(2)
            .Select(m => m.Content)
            .ToListAsync(ct);

        // 2. Contexto de negocio: hábitos activos del usuario
        var activeHabits = await db.Habits
            .AsNoTracking()
            .Where(h => h.IsActive)
            .Select(h => $"{h.Title} (Racha: {h.CurrentStreak} días)")
            .ToListAsync(ct);

        // 3. Ventana deslizante (Short-term context: buffer de mensajes posteriores al último resumen)
        var recentHistory = await db.ChatMessages
            .AsNoTracking()
            .Where(m =>
                m.ChatSessionId == chatSession.Id &&
                m.Id != userMessage.Id &&
                (chatSession.LastSummarizedAt == null || m.CreatedAt > chatSession.LastSummarizedAt))
            .OrderByDescending(m => m.CreatedAt)
            .Take(_options.MaxRecentHistoryCeiling)
            .Select(m => new ChatMessageItem(m.Role, m.Content))
            .ToListAsync(ct);

        recentHistory.Reverse();

        // 4. Inferencia con LLM
        var aiRequest = new AiServiceRequest(
            SessionId: chatSession.Id,
            UserMessage: userMessage.Content,
            History: recentHistory,
            CurrentSummary: chatSession.Summary,
            RelevantMemories: retrievedFacts,
            ActiveHabits: activeHabits
        );

        var aiResponse = await llmClient.GenerateResponseAsync(aiRequest, ct);

        var assistantMessage = new ChatMessage
        {
            ChatSessionId = chatSession.Id,
            Role = MessageRole.Assistant,
            Content = aiResponse.Answer,
            CreatedAt = DateTime.UtcNow
        };

        db.ChatMessages.Add(assistantMessage);
        await db.SaveChangesAsync(ct);

        logger.LogDebug("Chat procesado. Sesión: {SessionId}. Memorias RAG: {MemoryCount}. Tokens: {PromptTokens}",
            chatSession.Id,
            retrievedFacts.Count,
            aiResponse.PromptTokens);

        // 5. Evaluación de umbral para resumen asíncrono
        var unsummarizedMessagesCount = await db.ChatMessages
            .CountAsync(m =>
                m.ChatSessionId == chatSession.Id &&
                (chatSession.LastSummarizedAt == null || m.CreatedAt > chatSession.LastSummarizedAt),
                ct);

        if (unsummarizedMessagesCount >= _options.TriggerThreshold)
        {
            logger.LogInformation("Resumen encolado. Sesión: {SessionId}. Mensajes: {MessageCount}. Umbral: {Threshold}",
                chatSession.Id,
                unsummarizedMessagesCount,
                _options.TriggerThreshold);
            summaryQueue.TryEnqueue(chatSession.Id);
        }

        return new SendMessageResponse(
            Id: assistantMessage.Id,
            SessionId: assistantMessage.ChatSessionId,
            Role: assistantMessage.Role,
            Content: assistantMessage.Content,
            CreatedAt: assistantMessage.CreatedAt
        );
    }

    private async Task<ChatSession> GetOrCreateSessionAsync(
        Guid? sessionId,
        string initialMessage,
        CancellationToken ct)
    {
        if (sessionId.HasValue)
        {
            var existingSession = await db.ChatSessions.FindAsync([sessionId.Value], ct);
            if (existingSession is not null)
            {
                return existingSession;
            }
        }

        var newSession = new ChatSession
        {
            Id = sessionId ?? Guid.NewGuid(),
            Title = initialMessage.Length > 50
                ? $"{initialMessage[..47]}..."
                : initialMessage,
            CreatedAt = DateTime.UtcNow
        };

        db.ChatSessions.Add(newSession);
        return newSession;
    }
}
