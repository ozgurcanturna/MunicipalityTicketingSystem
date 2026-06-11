namespace SharedKernel.Domain.Events;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
    string EventType => GetType().Name;
}

public abstract class DomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed class EntityCreatedEvent : DomainEvent
{
    public Guid EntityId { get; }
    public EntityCreatedEvent(Guid entityId) => EntityId = entityId;
}

public sealed class EntityDeletedEvent : DomainEvent
{
    public Guid EntityId { get; }
    public EntityDeletedEvent(Guid entityId) => EntityId = entityId;
}

public sealed class EntityUpdatedEvent : DomainEvent
{
    public Guid EntityId { get; }
    public EntityUpdatedEvent(Guid entityId) => EntityId = entityId;
}
