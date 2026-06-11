using Microsoft.EntityFrameworkCore;
using SharedKernel.Domain.Entities;

namespace SharedKernel.Infrastructure.Persistence;

public abstract class AppDbContext : DbContext
{
    protected AppDbContext(DbContextOptions options) : base(options)
    {
    }

    public override int SaveChanges()
    {
        UpdateAuditableEntities();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditableEntities();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditableEntities()
    {
        var now = DateTime.UtcNow;
        var modifiedEntities = ChangeTracker.Entries<Entity>()
            .Where(e => e.State == EntityState.Modified)
            .Select(e => e.Entity);

        foreach (var entity in modifiedEntities)
        {
            entity.GetType().GetProperty("UpdatedAt")?.SetValue(entity, now);
        }
    }

    protected virtual void ConfigureMultiTenancy(ModelBuilder modelBuilder)
    {
        // Multi-tenancy desteği için alt sınıflar tarafından override edilir
    }
}
