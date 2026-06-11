using Microsoft.EntityFrameworkCore;
using SharedKernel.Domain.Entities;

namespace SharedKernel.Infrastructure.Persistence;

public abstract class AppDbContext : DbContext
{
    protected AppDbContext(DbContextOptions options)
        : base(options)
    {
    }

    public override int SaveChanges()
    {
        UpdateAuditableEntities();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        UpdateAuditableEntities();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditableEntities();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        UpdateAuditableEntities();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ConfigureMultiTenancy(modelBuilder);
    }

    protected virtual void ConfigureMultiTenancy(ModelBuilder modelBuilder)
    {
    }

    private void UpdateAuditableEntities()
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(entity => entity.CreatedAt).CurrentValue = utcNow;
                entry.Property(entity => entity.UpdatedAt).CurrentValue = null;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Property(entity => entity.UpdatedAt).CurrentValue = utcNow;
            }
        }
    }
}