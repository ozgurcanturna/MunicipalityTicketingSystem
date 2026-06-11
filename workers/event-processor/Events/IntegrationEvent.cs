namespace Journey.EventProcessor.Worker.Events;

public sealed record IntegrationEvent(
    Guid EventId,
    string TenantId,
    string EventType,
    string Payload,
    DateTime OccurredAt,
    string CorrelationId)
{
    public static IntegrationEvent Create(string tenantId, string eventType, string payload)
    {
        return new IntegrationEvent(
            Guid.NewGuid(),
            tenantId,
            eventType,
            payload,
            DateTime.UtcNow,
            Guid.NewGuid().ToString("N"));
    }
}