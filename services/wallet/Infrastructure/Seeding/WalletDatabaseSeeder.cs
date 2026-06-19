using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SharedKernel.Infrastructure.Persistence;
using Ticketing.Wallet.Api.Domain.Entities;
using Ticketing.Wallet.Api.Infrastructure.Persistence;

namespace Ticketing.Wallet.Api.Infrastructure.Seeding;

public sealed class WalletDatabaseSeeder
{
    private static readonly Guid BursaTenantId = Guid.Parse("7f4c8c0f-1d7b-4d52-8a4d-000000000001");

    private readonly IConfiguration _configuration;
    private readonly ILogger<WalletDatabaseSeeder> _logger;

    public WalletDatabaseSeeder(IConfiguration configuration, ILogger<WalletDatabaseSeeder> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var connectionString = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:Default ayarı zorunludur.");
        }

        PostgresDatabaseInitializer.EnsureDatabaseExists(connectionString);

        var options = new DbContextOptionsBuilder<WalletDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        await using var dbContext = new WalletDbContext(options);

        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        // Create wallet for Bursa tenant if not exists
        var existingWallet = await dbContext.Wallets
            .FirstOrDefaultAsync(w => w.TenantId == BursaTenantId, cancellationToken);

        if (existingWallet is null)
        {
            var wallet = WalletAccount.Create(BursaTenantId);

            // Add demo transactions
            var topUpRef1 = $"initial-topup-{DateTime.UtcNow:yyyyMMdd}";
            wallet.TopUp(500m, topUpRef1);

            var topUpRef2 = $"credit-added-{DateTime.UtcNow:yyyyMMdd}";
            wallet.TopUp(250m, topUpRef2);

            dbContext.Wallets.Add(wallet);

            _logger.LogInformation("Seed wallet created for Bursa tenant: {WalletId}", wallet.Id);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}