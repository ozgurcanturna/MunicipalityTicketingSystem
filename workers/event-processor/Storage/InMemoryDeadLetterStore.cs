using System.Collections.Concurrent;
using Journey.EventProcessor.Worker.Events;

namespace Journey.EventProcessor.Worker.Storage;

public sealed class InMemoryDeadLetterStore : IDeadLetterStore
{
    private readonly ConcurrentQueue<(IntegrationEvent Event, string Reason, DateTime StoredAt)> _items = new();

    public Task StoreAsync(IntegrationEvent integrationEvent, string reason, CancellationToken cancellationToken = default)
    {
        _items.Enqueue((integrationEvent, reason, DateTime.UtcNow));
        return Task.CompletedTask;
    }
}