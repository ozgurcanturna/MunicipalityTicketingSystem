using Tenant.Identity.Api.Domain.Entities;

namespace Tenant.Identity.Api.Application.Contracts;

public sealed record TenantResponse(
    Guid Id,
    string Name,
    bool IsActive,
    IReadOnlyCollection<UserResponse> Users)
{
    public static TenantResponse FromDomain(MunicipalityTenant tenant)
    {
        return new TenantResponse(
            tenant.Id,
            tenant.Name,
            tenant.IsActive,
            tenant.Users.Select(UserResponse.FromDomain).ToArray());
    }
}