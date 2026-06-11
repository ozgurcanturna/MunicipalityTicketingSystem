namespace SharedKernel.Domain.Entities;

using SharedKernel.Domain.Events;

public abstract class Entity
{
    private List<IDomainEvent>? _domainEvents;
    
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents?.AsReadOnly() ?? new List<IDomainEvent>();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents ??= new List<IDomainEvent>();
        _domainEvents.Add(domainEvent);
    }

    public IEnumerable<IDomainEvent> ClearDomainEvents()
    {
        if (_domainEvents == null || _domainEvents.Count == 0)
            return Array.Empty<IDomainEvent>();

        var events = _domainEvents.ToList();
        _domainEvents.Clear();
        return events;
    }

    protected void MarkAsModified()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    // Equality operators
    public override bool Equals(object? obj)
    {
        if (obj is not Entity other || GetType() != obj.GetType())
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (Id == default || other.Id == default)
            return false;

        return Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(Entity left, Entity right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(Entity left, Entity right)
    {
        return !(left == right);
    }
}
