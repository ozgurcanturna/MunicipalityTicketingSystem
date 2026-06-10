namespace SharedKernel.Domain.Entities;

/// <summary>
/// Tüm domain entity'leri için temel soyut sınıf.
/// DDD prensiplerine göre her varlığın bir kimliği (Id) olmalıdır.
/// </summary>
public abstract class Entity
{
    // Domain eventleri listesi
    private List<IDomainEvent>? _domainEvents;

    /// <summary>
    /// Entity'nin benzersiz kimliği.
    /// </summary>
    public Guid Id { get; protected set; } = Guid.NewGuid();

    /// <summary>
    /// Oluşturulma zamanı.
    /// </summary>
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Son güncellenme zamanı.
    /// </summary>
    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>
    /// Entity'ye ait domain eventlerinin salt okunur koleksiyonu.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents?.AsReadOnly() ?? new List<IDomainEvent>();

    /// <summary>
    /// Yeni bir domain event ekler.
    /// </summary>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents ??= new List<IDomainEvent>();
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Eklenen tüm domain eventlerini temizler ve döndürür.
    /// Genellikle event publisher tarafından kullanılır.
    /// </summary>
    public IEnumerable<IDomainEvent> ClearDomainEvents()
    {
        var dequeuedEvents = _domainEvents?.ToList() ?? new List<IDomainEvent>();
        _domainEvents?.Clear();
        return dequeuedEvents;
    }

    /// <summary>
    /// Güncellenme zamanını şu anki UTC zamana ayarlar.
    /// </summary>
    protected void MarkAsModified()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// İki entity'nin eşit olup olmadığını kontrol eder.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not Entity other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        if (Id == Guid.Empty || other.Id == Guid.Empty)
            return false;

        return Id == other.Id;
    }

    /// <summary>
    /// Hash kodunu oluşturur.
    /// </summary>
    public override int GetHashCode()
    {
        return (GetType().ToString() + Id).GetHashCode();
    }

    /// <summary>
    /// Eşitlik operatörü.
    /// </summary>
    public static bool operator ==(Entity left, Entity right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    /// <summary>
    /// Eşitsizlik operatörü.
    /// </summary>
    public static bool operator !=(Entity left, Entity right)
    {
        return !(left == right);
    }
}
