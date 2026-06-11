namespace Journey.EventProcessor.Worker.Events;

public interface IEventQueue
{
    Task EnqueueAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
    Task<IntegrationEvent?> DequeueAsync(CancellationToken cancellationToken = default);
}