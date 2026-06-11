namespace Journey.EventProcessor.Worker.Configuration;

public sealed class EventBusOptions
{
    public const string SectionName = "EventBus";

    public string Provider { get; init; } = "InMemory";
    public string Host { get; init; } = "rabbitmq";
    public int Port { get; init; } = 5672;
    public string UserName { get; init; } = "guest";
    public string Password { get; init; } = "guest";
    public string ExchangeName { get; init; } = "municipality.events";
    public string QueueName { get; init; } = "journey.event-processor";
    public string DeadLetterQueueName { get; init; } = "journey.event-processor.dlq";
    public ushort PrefetchCount { get; init; } = 20;
}