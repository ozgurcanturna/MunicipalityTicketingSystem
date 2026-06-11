using Journey.EventProcessor.Worker.Events;

namespace Journey.EventProcessor.Worker.Processing;

public sealed class WalletDebitedEventHandler(ILogger<WalletDebitedEventHandler> logger)
    : IIntegrationEventHandler
{
    public string EventType => "Wallet.Debited";

    public Task HandleAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Handled wallet-debited event. Tenant={TenantId}, Payload={Payload}",
            integrationEvent.TenantId,
            integrationEvent.Payload);
        return Task.CompletedTask;
    }
}