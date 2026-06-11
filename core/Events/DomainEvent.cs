namespace SharedKernel.Domain.Events;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}

public abstract class DomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; protected set; } = DateTime.UtcNow;
}

public class EntityCreatedEvent : DomainEvent
{
    public Guid EntityId { get; }

    public EntityCreatedEvent(Guid entityId)
    {
        EntityId = entityId;
    }
}

public class EntityDeletedEvent : DomainEvent
{
    public Guid EntityId { get; }

    public EntityDeletedEvent(Guid entityId)
    {
        EntityId = entityId;
    }
}
