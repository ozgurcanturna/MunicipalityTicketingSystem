namespace Tenant.Identity.Api.Application.Contracts;

public sealed record TokenResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    Guid UserId,
    Guid TenantId,
    string Email,
    string Role);