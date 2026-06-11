namespace Journey.EventProcessor.Worker.Processing;

public sealed class EventHandlerResolver : IEventHandlerResolver
{
    private readonly IReadOnlyDictionary<string, IIntegrationEventHandler> _handlers;

    public EventHandlerResolver(IEnumerable<IIntegrationEventHandler> handlers)
    {
        _handlers = handlers.ToDictionary(handler => handler.EventType, StringComparer.OrdinalIgnoreCase);
    }

    public IIntegrationEventHandler? Resolve(string eventType)
    {
        _handlers.TryGetValue(eventType, out var handler);
        return handler;
    }
}