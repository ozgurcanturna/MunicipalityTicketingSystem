using Journey.EventProcessor.Worker.Events;

namespace Journey.EventProcessor.Worker.Processing;

public sealed class IdentityTenantCreatedEventHandler(ILogger<IdentityTenantCreatedEventHandler> logger)
    : IIntegrationEventHandler
{
    public string EventType => "Identity.TenantCreated";

    public Task HandleAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Handled tenant-created event. Tenant={TenantId}, Payload={Payload}",
            integrationEvent.TenantId,
            integrationEvent.Payload);
        return Task.CompletedTask;
    }
}