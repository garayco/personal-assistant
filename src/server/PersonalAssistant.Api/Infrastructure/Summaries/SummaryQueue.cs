namespace PersonalAssistant.Api.Infrastructure.Summaries;

using System.Collections.Concurrent;
using System.Threading.Channels;

public sealed class SummaryQueue : ISummaryQueue
{
    private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>(new UnboundedChannelOptions
    {
        SingleReader = true
    });
    private readonly ConcurrentDictionary<Guid, byte> _activeSessions = new();

    public bool TryEnqueue(Guid sessionId)
    {
        if (!_activeSessions.TryAdd(sessionId, 0))
        {
            return false;
        }

        if (!_channel.Writer.TryWrite(sessionId))
        {
            _activeSessions.TryRemove(sessionId, out _);
            return false;
        }

        return true;
    }

    public IAsyncEnumerable<Guid> ReadAllAsync(CancellationToken ct)
    {
        return _channel.Reader.ReadAllAsync(ct);
    }

    public void MarkCompleted(Guid sessionId)
    {
        _activeSessions.TryRemove(sessionId, out _);
    }
}
