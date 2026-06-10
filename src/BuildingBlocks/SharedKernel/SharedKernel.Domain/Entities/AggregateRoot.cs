namespace SharedKernel.Domain.Entities;

/// <summary>
/// Aggregate Root temel sınıfı.
/// DDD'de Aggregate Root, bir aggregate'ın giriş noktasıdır ve tutarlılık sınırlarını belirler.
/// Tüm iş kuralları ve invariantlar Aggregate Root üzerinden yönetilir.
/// </summary>
public abstract class AggregateRoot : Entity
{
    // Aggregate seviyesinde version takibi (Optimistic concurrency için kullanılabilir)
    public long Version { get; protected set; } = 1;

    /// <summary>
    /// Aggregate root üzerinde yapılan değişiklikleri işaretler.
    /// </summary>
    protected void RaiseVersion()
    {
        Version++;
        MarkAsModified();
    }

    /// <summary>
    /// Yeni bir aggregate oluşturulduğunda tetiklenen event.
    /// </summary>
    protected void RegisterCreated()
    {
        AddDomainEvent(new DomainEvents.EntityCreatedEvent(Id));
    }

    /// <summary>
    /// Aggregate silindiğinde tetiklenen event.
    /// </summary>
    protected void RegisterDeleted()
    {
        AddDomainEvent(new DomainEvents.EntityDeletedEvent(Id));
    }
}
