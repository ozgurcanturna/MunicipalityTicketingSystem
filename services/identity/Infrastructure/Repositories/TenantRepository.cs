using Microsoft.EntityFrameworkCore;
using SharedKernel.Infrastructure.Repositories;
using Tenant.Identity.Api.Application.Repositories;
using Tenant.Identity.Api.Domain.Entities;
using Tenant.Identity.Api.Infrastructure.Persistence;

namespace Tenant.Identity.Api.Infrastructure.Repositories;

public sealed class TenantRepository : Repository<MunicipalityTenant>, ITenantRepository
{
    private static readonly Dictionary<string, Guid> SlugToGuid = new()
    {
        ["bursa"] = Guid.Parse("7f4c8c0f-1d7b-4d52-8a4d-000000000001"),
        ["eskisehir"] = Guid.Parse("7f4c8c0f-1d7b-4d52-8a4d-000000000002"),
        ["van"] = Guid.Parse("7f4c8c0f-1d7b-4d52-8a4d-000000000003"),
        ["mersin"] = Guid.Parse("7f4c8c0f-1d7b-4d52-8a4d-000000000004"),
    };

    public TenantRepository(IdentityDbContext dbContext)
        : base(dbContext)
    {
    }

    private IdentityDbContext IdentityDbContext => (IdentityDbContext)DbContext;

    public new Task<MunicipalityTenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return IdentityDbContext.Tenants
            .Include(tenant => tenant.Users)
            .FirstOrDefaultAsync(tenant => tenant.Id == id, cancellationToken);
    }

    public Task<MunicipalityTenant?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return IdentityDbContext.Tenants
            .Include(tenant => tenant.Users)
            .FirstOrDefaultAsync(
                tenant => tenant.Name == name,
                cancellationToken);
    }

    public Task<MunicipalityTenant?> GetByUserEmailAsync(Guid tenantId, string email, CancellationToken cancellationToken = default)
    {
        return IdentityDbContext.Tenants
            .Include(tenant => tenant.Users)
            .FirstOrDefaultAsync(
                tenant => tenant.Id == tenantId
                    && tenant.Users.Any(user => user.Email == email),
                cancellationToken);
    }

    public Task<MunicipalityTenant?> GetByUserEmailAsync(string tenantSlug, string email, CancellationToken cancellationToken = default)
    {
        var tenantId = SlugToGuid.GetValueOrDefault(tenantSlug.ToLowerInvariant());
        return GetByUserEmailAsync(tenantId, email, cancellationToken);
    }
}