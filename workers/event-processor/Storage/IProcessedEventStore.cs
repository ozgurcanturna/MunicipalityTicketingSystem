namespace Journey.EventProcessor.Worker.Storage;

public interface IProcessedEventStore
{
    bool IsProcessed(Guid eventId);
    void MarkProcessed(Guid eventId);
}