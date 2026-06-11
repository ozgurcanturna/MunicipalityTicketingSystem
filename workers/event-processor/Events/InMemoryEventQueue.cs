using System.Threading.Channels;

namespace Journey.EventProcessor.Worker.Events;

public sealed class InMemoryEventQueue : IEventQueue
{
    private readonly Channel<IntegrationEvent> _channel = Channel.CreateUnbounded<IntegrationEvent>();

    public Task EnqueueAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        return _channel.Writer.WriteAsync(integrationEvent, cancellationToken).AsTask();
    }

    public async Task<IntegrationEvent?> DequeueAsync(CancellationToken cancellationToken = default)
    {
        if (await _channel.Reader.WaitToReadAsync(cancellationToken))
        {
            return await _channel.Reader.ReadAsync(cancellationToken);
        }

        return null;
    }
}