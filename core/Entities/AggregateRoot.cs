namespace SharedKernel.Domain.Entities;

using SharedKernel.Domain.Events;

public abstract class AggregateRoot : Entity
{
    public long Version { get; private set; } = 1;

    private readonly List<IDomainEvent> _uncommittedEvents = new();
    public IReadOnlyCollection<IDomainEvent> UncommittedEvents => _uncommittedEvents.AsReadOnly();

    protected override void AddDomainEvent(IDomainEvent domainEvent)
    {
        base.AddDomainEvent(domainEvent);
        _uncommittedEvents.Add(domainEvent);
    }

    public void MarkEventsAsCommitted()
    {
        _uncommittedEvents.Clear();
        Version++;
    }

    protected void RegisterCreated()
    {
        AddDomainEvent(new EntityCreatedEvent(Id));
    }

    protected void RegisterDeleted()
    {
        AddDomainEvent(new EntityDeletedEvent(Id));
    }
}
