using Journey.EventProcessor.Worker.Events;

namespace Journey.EventProcessor.Worker.Processing;

public sealed class JourneyCompletedEventHandler(ILogger<JourneyCompletedEventHandler> logger)
    : IIntegrationEventHandler
{
    public string EventType => "Telemetry.JourneyCompleted";

    public Task HandleAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Handled journey-completed event. Tenant={TenantId}, Payload={Payload}",
            integrationEvent.TenantId,
            integrationEvent.Payload);
        return Task.CompletedTask;
    }
}