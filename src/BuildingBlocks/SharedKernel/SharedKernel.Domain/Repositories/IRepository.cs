using SharedKernel.Domain.Entities;

namespace SharedKernel.Domain.Repositories;

/// <summary>
/// Generic repository interface.
/// DDD'de repository pattern, domain entity'lerinin persistence katmanıyla etkileşimini soyutlar.
/// </summary>
public interface IRepository<TEntity> where TEntity : AggregateRoot
{
    /// <summary>
    /// Belirtilen ID'ye sahip entity'yi asenkron olarak getirir.
    /// Bulunamazsa null döner.
    /// </summary>
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tüm entity'leri asenkron olarak listeler.
    /// </summary>
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Yeni bir entity ekler.
    /// </summary>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mevcut bir entity'yi günceller.
    /// </summary>
    void Update(TEntity entity);

    /// <summary>
    /// Bir entity'yi siler.
    /// </summary>
    void Delete(TEntity entity);

    /// <summary>
    /// Belirtilen predicate'e göre entity'leri filtreler.
    /// </summary>
    Task<IEnumerable<TEntity>> FindAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unit of Work ile yapılan değişiklikleri kaydeder.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Non-generic repository interface for advanced scenarios.
/// </summary>
public interface IRepository
{
    /// <summary>
    /// Unit of Work ile yapılan değişiklikleri kaydeder.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
