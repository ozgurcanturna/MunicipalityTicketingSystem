using Microsoft.EntityFrameworkCore;
using SharedKernel.Domain.Repositories;

namespace SharedKernel.Infrastructure.Persistence;

public abstract class AppDbContext : DbContext, IUnitOfWork
{
    protected AppDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Tüm entity'ler için ortak yapılandırmalar burada uygulanabilir
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Domain event dispatching buraya eklenecek
        return base.SaveChangesAsync(cancellationToken);
    }
}
