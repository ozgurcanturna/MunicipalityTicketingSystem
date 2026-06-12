using Journey.EventProcessor.Worker.Configuration;
using Journey.EventProcessor.Worker.Events;
using Journey.EventProcessor.Worker.Processing;
using Journey.EventProcessor.Worker.Storage;
using Microsoft.Extensions.Options;

namespace Journey.EventProcessor.Worker;

public sealed class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventQueue _eventQueue;
    private readonly IProcessedEventStore _processedEventStore;
    private readonly IDeadLetterStore _deadLetterStore;
    private readonly EventProcessorOptions _options;
    private readonly EventBusOptions _eventBusOptions;

    public Worker(
        ILogger<Worker> logger,
        IServiceProvider serviceProvider,
        IEventQueue eventQueue,
        IProcessedEventStore processedEventStore,
        IDeadLetterStore deadLetterStore,
        IOptions<EventProcessorOptions> options,
        IOptions<EventBusOptions> eventBusOptions)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _eventQueue = eventQueue;
        _processedEventStore = processedEventStore;
        _deadLetterStore = deadLetterStore;
        _options = options.Value;
        _eventBusOptions = eventBusOptions.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Event processor started. Stack={Stack}, Transport={Transport}, Exchange={Exchange}, Queue={Queue}, DLQ={DeadLetterQueue}, MaxRetry={MaxRetry}",
            _eventBusOptions.Stack,
            _eventBusOptions.Transport,
            _eventBusOptions.ExchangeName,
            _eventBusOptions.QueueName,
            _eventBusOptions.DeadLetterQueueName,
            _options.MaxRetryCount);

        await SeedEventsIfEnabledAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var integrationEvent = await _eventQueue.DequeueAsync(stoppingToken);
            if (integrationEvent is null)
            {
                await Task.Delay(100, stoppingToken);
                continue;
            }

            await ProcessEventAsync(integrationEvent, stoppingToken);
        }
    }

    private async Task ProcessEventAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        if (_processedEventStore.IsProcessed(integrationEvent.EventId))
        {
            _logger.LogInformation("Event {EventId} already processed, skipping.", integrationEvent.EventId);
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var resolver = scope.ServiceProvider.GetRequiredService<IEventHandlerResolver>();
        var handler = resolver.Resolve(integrationEvent.EventType);

        if (handler is null)
        {
            _logger.LogWarning(
                "No handler found for EventType {EventType}. EventId={EventId}",
                integrationEvent.EventType,
                integrationEvent.EventId);
            await _deadLetterStore.StoreAsync(integrationEvent, "HandlerNotFound", cancellationToken);
            return;
        }

        var success = await ExecuteWithRetryAsync(
            async () => await handler.HandleAsync(integrationEvent, cancellationToken),
            cancellationToken);

        if (success)
        {
            _processedEventStore.MarkProcessed(integrationEvent.EventId);
            _logger.LogInformation(
                "Processed event {EventId} ({EventType}) for tenant {TenantId}",
                integrationEvent.EventId,
                integrationEvent.EventType,
                integrationEvent.TenantId);
            return;
        }

        await _deadLetterStore.StoreAsync(integrationEvent, "RetryLimitExceeded", cancellationToken);
        _logger.LogError(
            "Event moved to dead-letter store. EventId={EventId}, EventType={EventType}",
            integrationEvent.EventId,
            integrationEvent.EventType);
    }

    private async Task<bool> ExecuteWithRetryAsync(Func<Task> action, CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= _options.MaxRetryCount; attempt++)
        {
            try
            {
                await action();
                return true;
            }
            catch (Exception exception) when (attempt < _options.MaxRetryCount)
            {
                var delayMilliseconds = _options.BaseRetryDelayMs * attempt;
                _logger.LogWarning(
                    exception,
                    "Event processing failed at attempt {Attempt}/{MaxAttempt}. Retrying in {Delay}ms.",
                    attempt,
                    _options.MaxRetryCount,
                    delayMilliseconds);

                await Task.Delay(delayMilliseconds, cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Event processing failed at final attempt {Attempt}/{MaxAttempt}.",
                    attempt,
                    _options.MaxRetryCount);
                return false;
            }
        }

        return false;
    }

    private async Task SeedEventsIfEnabledAsync(CancellationToken cancellationToken)
    {
        if (!_options.SeedDemoEvents)
        {
            return;
        }

        var demoTenantId = "bursa";

        await _eventQueue.EnqueueAsync(
            IntegrationEvent.Create(demoTenantId, "Identity.TenantCreated", "{\"tenantName\":\"Bursa\"}"),
            cancellationToken);

        await _eventQueue.EnqueueAsync(
            IntegrationEvent.Create(demoTenantId, "Wallet.Debited", "{\"amount\":12.50,\"cardId\":\"CARD-1001\"}"),
            cancellationToken);

        await _eventQueue.EnqueueAsync(
            IntegrationEvent.Create(demoTenantId, "Telemetry.JourneyCompleted", "{\"journeyId\":\"DEMO-JRNY-1\"}"),
            cancellationToken);

        _logger.LogInformation("Seeded demo events into in-memory queue.");
    }
}
