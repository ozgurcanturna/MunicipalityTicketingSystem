using Journey.EventProcessor.Worker.Events;

namespace Journey.EventProcessor.Worker.Storage;

public interface IDeadLetterStore
{
    Task StoreAsync(IntegrationEvent integrationEvent, string reason, CancellationToken cancellationToken = default);
}