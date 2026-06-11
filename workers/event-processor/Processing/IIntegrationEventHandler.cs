using Journey.EventProcessor.Worker.Events;

namespace Journey.EventProcessor.Worker.Processing;

public interface IIntegrationEventHandler
{
    string EventType { get; }
    Task HandleAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken);
}