using Microsoft.EntityFrameworkCore;

namespace SharedKernel.Infrastructure.Persistence;

/// <summary>
/// Temel DbContext sınıfı.
/// Tüm mikroservislerin context sınıfları bu sınıftan türer.
/// </summary>
public abstract class AppDbContext : DbContext
{
    protected AppDbContext(DbContextOptions options) : base(options)
    {
    }

    /// <summary>
    /// Her entity için CreatedAt ve UpdatedAt alanlarını otomatik doldurur.
    /// </summary>
    public override int SaveChanges()
    {
        UpdateAuditableEntities();
        return base.SaveChanges();
    }

    /// <summary>
    /// Her entity için CreatedAt ve UpdatedAt alanlarını otomatik doldurur.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditableEntities();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Auditable entity'lerin zaman damgalarını günceller.
    /// </summary>
    private void UpdateAuditableEntities()
    {
        var now = DateTime.UtcNow;

        var addedEntities = ChangeTracker.Entries<Domain.Entities.Entity>()
            .Where(e => e.State == EntityState.Added && e.Entity.CreatedAt == default)
            .Select(e => e.Entity);

        foreach (var entity in addedEntities)
        {
            // CreatedAt zaten Entity constructor'ında set ediliyor
        }

        var modifiedEntities = ChangeTracker.Entries<Domain.Entities.Entity>()
            .Where(e => e.State == EntityState.Modified)
            .Select(e => e.Entity);

        foreach (var entity in modifiedEntities)
        {
            entity.GetType().GetProperty("UpdatedAt")?.SetValue(entity, now);
        }
    }

    /// <summary>
    /// Model oluşturulurken yapılacak yapılandırmalar için hook.
    /// </summary>
    protected virtual void ConfigureMultiTenancy(ModelBuilder modelBuilder)
    {
        // Multi-tenancy desteği için alt sınıflar tarafından override edilir
    }
}
