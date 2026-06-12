using Microsoft.EntityFrameworkCore;
using Tenant.Identity.Api.Domain.Entities;
using Tenant.Identity.Api.Infrastructure.Authentication;
using Tenant.Identity.Api.Infrastructure.Persistence;

namespace Tenant.Identity.Api.Infrastructure.Seeding;

public sealed class IdentityDatabaseSeeder
{
    private readonly IConfiguration _configuration;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<IdentityDatabaseSeeder> _logger;

    public IdentityDatabaseSeeder(
        IConfiguration configuration,
        IPasswordHasher passwordHasher,
        ILogger<IdentityDatabaseSeeder> logger)
    {
        _configuration = configuration;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var connectionString = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:Default ayarı zorunludur.");
        }

        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using var dbContext = new IdentityDbContext(options);

        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        foreach (var tenantSeed in IdentitySeedCatalog.DemoTenants)
        {
            await SeedTenantAsync(dbContext, tenantSeed, cancellationToken);
        }
    }

    private async Task SeedTenantAsync(
        IdentityDbContext dbContext,
        SeedTenantDefinition tenantSeed,
        CancellationToken cancellationToken)
    {
        var tenant = await dbContext.Tenants
            .Include(existingTenant => existingTenant.Users)
            .FirstOrDefaultAsync(existingTenant => existingTenant.Id == tenantSeed.TenantId, cancellationToken);

        if (tenant is null)
        {
            tenant = MunicipalityTenant.Create(tenantSeed.TenantId, tenantSeed.TenantName);
            dbContext.Tenants.Add(tenant);
            _logger.LogInformation("Seed tenant created: {TenantName} ({TenantId})", tenantSeed.TenantName, tenantSeed.TenantId);
        }

        var existingEmails = tenant.Users
            .Select(user => user.Email)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var userSeed in tenantSeed.Users)
        {
            if (existingEmails.Contains(userSeed.Email))
            {
                continue;
            }

            tenant.AddUser(
                userSeed.Email,
                userSeed.FullName,
                _passwordHasher.Hash(userSeed.Password),
                userSeed.Role);

            _logger.LogInformation(
                "Seed user created: {Email} ({Role}) for tenant {TenantName}",
                userSeed.Email,
                userSeed.Role,
                tenantSeed.TenantName);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}