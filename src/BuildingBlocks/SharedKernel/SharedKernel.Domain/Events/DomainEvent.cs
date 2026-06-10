namespace SharedKernel.Domain.Events;

/// <summary>
/// Tüm domain eventleri için temel interface.
/// Domain eventler, domain katmanında gerçekleşen önemli olayları temsil eder.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Event'in oluştuğu zaman (UTC).
    /// </summary>
    DateTime OccurredOn { get; }

    /// <summary>
    /// Event türü adı.
    /// </summary>
    string EventType => GetType().Name;
}

/// <summary>
/// Temel domain event implementasyonu.
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

/// <summary>
/// Bir entity oluşturulduğunda tetiklenen event.
/// </summary>
public sealed class EntityCreatedEvent : DomainEvent
{
    public Guid EntityId { get; }

    public EntityCreatedEvent(Guid entityId)
    {
        EntityId = entityId;
    }
}

/// <summary>
/// Bir entity silindiğinde tetiklenen event.
/// </summary>
public sealed class EntityDeletedEvent : DomainEvent
{
    public Guid EntityId { get; }

    public EntityDeletedEvent(Guid entityId)
    {
        EntityId = entityId;
    }
}

/// <summary>
/// Bir entity güncellendiğinde tetiklenen event.
/// </summary>
public sealed class EntityUpdatedEvent : DomainEvent
{
    public Guid EntityId { get; }

    public EntityUpdatedEvent(Guid entityId)
    {
        EntityId = entityId;
    }
}
