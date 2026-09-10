namespace PersonalAssistant.Api.Infrastructure.Summaries;

public interface ISummaryQueue
{
    bool TryEnqueue(Guid sessionId);
    IAsyncEnumerable<Guid> ReadAllAsync(CancellationToken ct);
    void MarkCompleted(Guid sessionId);
}
