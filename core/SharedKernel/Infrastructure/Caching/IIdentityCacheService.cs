namespace SharedKernel.Infrastructure.Caching;

public interface IIdentityCacheService
{
    Task<string?> GetTenantConfigAsync(string tenantId, CancellationToken cancellationToken = default);

    Task SetTenantConfigAsync(string tenantId, string configJson, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    Task<string?> GetJwtTokenAsync(string userId, string tenantId, CancellationToken cancellationToken = default);

    Task SetJwtTokenAsync(string userId, string tenantId, string token, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    Task InvalidateTenantCacheAsync(string tenantId, CancellationToken cancellationToken = default);
}
