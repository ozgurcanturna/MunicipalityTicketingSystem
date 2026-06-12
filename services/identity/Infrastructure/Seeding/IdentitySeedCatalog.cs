using Tenant.Identity.Api.Domain.Constants;

namespace Tenant.Identity.Api.Infrastructure.Seeding;

public sealed record SeedUserDefinition(string Email, string FullName, string Password, string Role);

public sealed record SeedTenantDefinition(Guid TenantId, string TenantName, IReadOnlyCollection<SeedUserDefinition> Users);

public static class IdentitySeedCatalog
{
    public static readonly SeedTenantDefinition[] DemoTenants =
    [
        new(
            Guid.Parse("7f4c8c0f-1d7b-4d52-8a4d-000000000001"),
            "ankara",
            [
                new SeedUserDefinition("admin@ankara.local", "Ankara Admin", "P@ssw0rd!", IdentityRoles.Admin),
                new SeedUserDefinition("operator@ankara.local", "Ankara Operator", "P@ssw0rd!", IdentityRoles.Operator),
                new SeedUserDefinition("user@ankara.local", "Ankara User", "P@ssw0rd!", IdentityRoles.User)
            ])
    ];
}