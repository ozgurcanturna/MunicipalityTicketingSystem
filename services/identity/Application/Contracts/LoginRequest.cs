namespace Tenant.Identity.Api.Application.Contracts;

public sealed record LoginRequest(Guid TenantId, string Email, string Password);