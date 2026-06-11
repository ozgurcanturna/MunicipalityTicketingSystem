using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using core.SharedKernel.Domain.Entities;
using core.SharedKernel.Domain.Repositories;

using core.SharedKernel.Infrastructure.Persistence;

namespace core.SharedKernel.Infrastructure.Repositories;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : AggregateRoot
{
    protected readonly  AppDbContext _dbContext;

    public Repository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<TEntity>().FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<TEntity>().ToListAsync(cancellationToken);
    }

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
    }

    public virtual void Update(TEntity entity) => _dbContext.Set<TEntity>().Update(entity);
    
    public virtual void Delete(TEntity entity) => _dbContext.Set<TEntity>().Remove(entity);

    public virtual async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<TEntity>().Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
