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
        
        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Property(e => e.UpdatedAt).CurrentValue = now;
            }
        }
    }

    protected virtual void ConfigureMultiTenancy(ModelBuilder modelBuilder)
    {
        // Multi-tenancy desteği için alt sınıflar tarafından override edilir
    }
}
