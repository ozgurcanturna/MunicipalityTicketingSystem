namespace Tenant.Identity.Api.Application.Contracts;

public sealed record BootstrapTenantRequest(
    string TenantName,
    string AdminEmail,
    string AdminFullName,
    string AdminPassword);