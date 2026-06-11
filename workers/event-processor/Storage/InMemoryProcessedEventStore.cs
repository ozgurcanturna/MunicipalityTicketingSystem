using System.Collections.Concurrent;

namespace Journey.EventProcessor.Worker.Storage;

public sealed class InMemoryProcessedEventStore : IProcessedEventStore
{
    private readonly ConcurrentDictionary<Guid, byte> _processedEvents = new();

    public bool IsProcessed(Guid eventId)
    {
        return _processedEvents.ContainsKey(eventId);
    }

    public void MarkProcessed(Guid eventId)
    {
        _processedEvents.TryAdd(eventId, 0);
    }
}