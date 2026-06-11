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

public sealed class EntityCreatedEvent(Guid entityId) : DomainEvent
{
    public Guid EntityId { get; } = entityId;
}

public sealed class EntityUpdatedEvent(Guid entityId) : DomainEvent
{
    public Guid EntityId { get; } = entityId;
}

public sealed class EntityDeletedEvent(Guid entityId) : DomainEvent
{
    public Guid EntityId { get; } = entityId;
}