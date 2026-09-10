namespace PersonalAssistant.Api.Infrastructure.Summaries;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Pgvector.EntityFrameworkCore;
using PersonalAssistant.Api.Common.Contracts;
using PersonalAssistant.Api.Domain.Entities;
using PersonalAssistant.Api.Infrastructure.Database;
using PersonalAssistant.Api.Infrastructure.Llm;

public sealed class SummaryWorker(
    ISummaryQueue summaryQueue,
    IServiceScopeFactory scopeFactory,
    IOptions<SummaryOptions> options,
    ILogger<SummaryWorker> logger) : BackgroundService
{
    private readonly SummaryOptions _options = options.Value;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Summary worker iniciado.");

        await foreach (var sessionId in summaryQueue.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var llmClient = scope.ServiceProvider.GetRequiredService<ILlmClient>();

                var session = await db.ChatSessions.FindAsync([sessionId], stoppingToken);
                if (session is null) continue;

                var unsummarizedData = await db.ChatMessages
                    .AsNoTracking()
                    .Where(m => m.ChatSessionId == session.Id &&
                                (session.LastSummarizedAt == null || m.CreatedAt > session.LastSummarizedAt))
                    .OrderBy(m => m.CreatedAt)
                    .Select(m => new { m.Role, m.Content, m.CreatedAt })
                    .ToListAsync(stoppingToken);

                var messagesToSummarizeCount = unsummarizedData.Count - _options.SlidingWindowSize;
                if (messagesToSummarizeCount < _options.MinBatchToSummarize) continue;

                var messagesToSummarize = unsummarizedData.Take(messagesToSummarizeCount).ToList();

                // Capturamos la fecha exacta del último mensaje incluido en este lote
                var lastProcessedAt = messagesToSummarize[^1].CreatedAt;
                var unsummarizedMessages = messagesToSummarize
                    .Select(m => new ChatMessageItem(m.Role, m.Content))
                    .ToList();

                var existingMemories = await db.SessionMemories
                    .Where(m => m.ChatSessionId == session.Id)
                    .OrderBy(m => m.CreatedAt)
                    .ToListAsync(stoppingToken);

                var existingFacts = existingMemories.Select(m => m.Content).ToList();

                var summaryRequest = new SummaryRequest(session.Id, unsummarizedMessages, session.Summary, existingFacts);
                var summaryResult = await llmClient.GenerateSummaryAsync(summaryRequest, stoppingToken);

                session.Summary = summaryResult.Summary;
                session.LastSummarizedAt = lastProcessedAt;

                var savedFactsCount = 0;
                var updatedFactsCount = 0;

                // 1. Procesar hechos actualizados (mutaciones referenciadas por ID/índice 1-based)
                if (summaryResult.UpdatedFacts is { Count: > 0 })
                {
                    foreach (var update in summaryResult.UpdatedFacts)
                    {
                        var newText = update.Updated?.Trim();
                        if (string.IsNullOrWhiteSpace(newText)) continue;

                        var index = update.Id - 1;
                        SessionMemory? existingRecord = null;
                        if (index >= 0 && index < existingMemories.Count)
                        {
                            existingRecord = existingMemories[index];
                        }

                        var newEmbedding = await llmClient.GetEmbeddingAsync(newText, stoppingToken);

                        if (existingRecord is not null)
                        {
                            existingRecord.Content = newText;
                            existingRecord.Embedding = newEmbedding;
                            existingRecord.CreatedAt = DateTime.UtcNow;
                            updatedFactsCount++;
                        }
                        else
                        {
                            var newMemory = new SessionMemory
                            {
                                ChatSessionId = session.Id,
                                Content = newText,
                                Embedding = newEmbedding,
                                CreatedAt = DateTime.UtcNow
                            };
                            db.SessionMemories.Add(newMemory);
                            existingMemories.Add(newMemory);
                            savedFactsCount++;
                        }
                    }
                }

                // 2. Procesar hechos completamente nuevos
                foreach (var fact in summaryResult.NewFacts)
                {
                    var factText = fact.Trim();
                    if (string.IsNullOrWhiteSpace(factText)) continue;

                    var factEmbedding = await llmClient.GetEmbeddingAsync(factText, stoppingToken);

                    var isDuplicate = await db.SessionMemories
                        .AsNoTracking()
                        .AnyAsync(m => m.ChatSessionId == session.Id &&
                                       m.Embedding.CosineDistance(factEmbedding) < 0.15, stoppingToken);

                    if (!isDuplicate)
                    {
                        db.SessionMemories.Add(new SessionMemory
                        {
                            ChatSessionId = session.Id,
                            Content = factText,
                            Embedding = factEmbedding,
                            CreatedAt = DateTime.UtcNow
                        });
                        savedFactsCount++;
                    }
                }

                await db.SaveChangesAsync(stoppingToken);
                logger.LogInformation(
                    "Resumen completado. Sesión: {SessionId}. Mensajes: {MessageCount}. Hechos nuevos: {SavedFacts}. Hechos actualizados: {UpdatedFacts}",
                    session.Id,
                    messagesToSummarize.Count,
                    savedFactsCount,
                    updatedFactsCount);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al procesar el resumen para sesión {SessionId}.", sessionId);
            }
            finally
            {
                summaryQueue.MarkCompleted(sessionId);
            }
        }
    }
}
