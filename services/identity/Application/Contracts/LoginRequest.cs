namespace Tenant.Identity.Api.Application.Contracts;

/// <summary>
/// Login request using tenant slug (e.g., "bursa") for frontend compatibility
/// The slug is mapped to tenant GUID internally by TenantConnectionStringResolver
/// </summary>
public sealed record LoginRequest(string TenantId, string Email, string Password);