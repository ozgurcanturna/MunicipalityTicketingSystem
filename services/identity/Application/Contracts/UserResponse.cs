using Tenant.Identity.Api.Domain.Entities;

namespace Tenant.Identity.Api.Application.Contracts;

public sealed record UserResponse(
    Guid Id,
    Guid TenantId,
    string Email,
    string FullName,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt)
{
    public static UserResponse FromDomain(User user)
    {
        return new UserResponse(
            user.Id,
            user.TenantId,
            user.Email,
            user.FullName,
            user.IsActive,
            user.CreatedAt,
            user.LastLoginAt);
    }
}