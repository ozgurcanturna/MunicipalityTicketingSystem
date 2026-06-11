namespace Journey.EventProcessor.Worker.Processing;

public interface IEventHandlerResolver
{
    IIntegrationEventHandler? Resolve(string eventType);
}