using System.Linq.Expressions;
using SharedKernel.Domain.Entities;

namespace SharedKernel.Domain.Repositories;

public interface IRepository<TEntity> : IRepository where TEntity : AggregateRoot
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    void Delete(TEntity entity);
}

public interface IRepository
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}