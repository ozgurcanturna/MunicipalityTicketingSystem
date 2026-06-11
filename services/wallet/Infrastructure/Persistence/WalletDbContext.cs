using Microsoft.EntityFrameworkCore;
using SharedKernel.Infrastructure.Persistence;
using Ticketing.Wallet.Api.Domain.Entities;

namespace Ticketing.Wallet.Api.Infrastructure.Persistence;

public sealed class WalletDbContext : AppDbContext
{
    public WalletDbContext(DbContextOptions<WalletDbContext> options)
        : base(options)
    {
    }

    public DbSet<WalletAccount> Wallets => Set<WalletAccount>();
    public DbSet<WalletTransaction> Transactions => Set<WalletTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<WalletAccount>(entity =>
        {
            entity.ToTable("Wallets");
            entity.HasKey(wallet => wallet.Id);
            entity.Property(wallet => wallet.Balance).HasPrecision(18, 2);
            entity.HasIndex(wallet => wallet.TenantId).IsUnique();

            entity
                .HasMany(wallet => wallet.Transactions)
                .WithOne()
                .HasForeignKey(transaction => transaction.WalletId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WalletTransaction>(entity =>
        {
            entity.ToTable("WalletTransactions");
            entity.HasKey(transaction => transaction.Id);
            entity.Property(transaction => transaction.Amount).HasPrecision(18, 2);
            entity.Property(transaction => transaction.Type).HasMaxLength(32).IsRequired();
            entity.Property(transaction => transaction.Reference).HasMaxLength(128).IsRequired();
            entity.HasIndex(transaction => transaction.WalletId);
            entity.HasIndex(transaction => transaction.OccurredAt);
        });
    }
}