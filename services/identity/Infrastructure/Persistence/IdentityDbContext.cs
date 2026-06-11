using Microsoft.EntityFrameworkCore;
using SharedKernel.Infrastructure.Persistence;
using Tenant.Identity.Api.Domain.Entities;

namespace Tenant.Identity.Api.Infrastructure.Persistence;

public sealed class IdentityDbContext : AppDbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options)
    {
    }

    public DbSet<MunicipalityTenant> Tenants => Set<MunicipalityTenant>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MunicipalityTenant>(entity =>
        {
            entity.ToTable("Tenants");
            entity.HasKey(tenant => tenant.Id);
            entity.Property(tenant => tenant.Name).HasMaxLength(200).IsRequired();
            entity.HasIndex(tenant => tenant.Name).IsUnique();

            entity
                .HasMany(tenant => tenant.Users)
                .WithOne()
                .HasForeignKey(user => user.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Email).HasMaxLength(320).IsRequired();
            entity.Property(user => user.FullName).HasMaxLength(200).IsRequired();
            entity.Property(user => user.PasswordHash).HasMaxLength(512).IsRequired();
            entity.Property(user => user.Role).HasMaxLength(50).IsRequired();
            entity.HasIndex(user => new { user.TenantId, user.Email }).IsUnique();
        });
    }
}