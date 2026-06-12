using Tenant.Identity.Api.Domain.Constants;

namespace Tenant.Identity.Api.Infrastructure.Seeding;

public sealed record SeedUserDefinition(string Email, string FullName, string Password, string Role);

public sealed record SeedTenantDefinition(Guid TenantId, string TenantName, IReadOnlyCollection<SeedUserDefinition> Users);

public static class IdentitySeedCatalog
{
    public static readonly SeedTenantDefinition[] DemoTenants =
    [
        CreateTenant("7f4c8c0f-1d7b-4d52-8a4d-000000000001", "Bursa", "bursa"),
        CreateTenant("7f4c8c0f-1d7b-4d52-8a4d-000000000002", "Eskişehir", "eskisehir"),
        CreateTenant("7f4c8c0f-1d7b-4d52-8a4d-000000000003", "Van", "van"),
        CreateTenant("7f4c8c0f-1d7b-4d52-8a4d-000000000004", "Mersin", "mersin")
    ];

    private static SeedTenantDefinition CreateTenant(string tenantId, string tenantName, string slug)
    {
        return new SeedTenantDefinition(
            Guid.Parse(tenantId),
            tenantName,
            [
                new SeedUserDefinition($"admin@{slug}.local", $"{tenantName} Admin", "P@ssw0rd!", IdentityRoles.Admin),
                new SeedUserDefinition($"operator@{slug}.local", $"{tenantName} Operator", "P@ssw0rd!", IdentityRoles.Operator),
                new SeedUserDefinition($"user@{slug}.local", $"{tenantName} User", "P@ssw0rd!", IdentityRoles.User)
            ]);
    }
}