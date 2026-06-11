using SharedKernel.Domain.Events;

namespace SharedKernel.Domain.Entities;

public abstract class AggregateRoot : Entity
{
    public long Version { get; protected set; } = 1;

    protected void RaiseVersion()
    {
        Version++;
        MarkAsModified();
    }

    protected void RegisterCreated()
    {
        AddDomainEvent(new EntityCreatedEvent(Id));
    }

    protected void RegisterUpdated()
    {
        RaiseVersion();
        AddDomainEvent(new EntityUpdatedEvent(Id));
    }

    protected void RegisterDeleted()
    {
        AddDomainEvent(new EntityDeletedEvent(Id));
    }
}