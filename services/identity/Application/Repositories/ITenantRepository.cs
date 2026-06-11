using SharedKernel.Domain.Repositories;
using Tenant.Identity.Api.Domain.Entities;

namespace Tenant.Identity.Api.Application.Repositories;

public interface ITenantRepository : IRepository<MunicipalityTenant>
{
    Task<MunicipalityTenant?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<MunicipalityTenant?> GetByUserEmailAsync(Guid tenantId, string email, CancellationToken cancellationToken = default);
}