using System;

namespace SharedKernel.Infrastructure.Cache;

/// <summary>
/// Identity service cache operations
/// </summary>
public interface IIdentityCacheService
{
    /// <summary>
    /// Get user token from cache
    /// </summary>
    Task<string?> GetTokenAsync(string userId, string tenantId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cache user token with expiration
    /// </summary>
    Task SetTokenAsync(string userId, string tenantId, string token, TimeSpan expiration, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Invalidate user token cache
    /// </summary>
    Task InvalidateTokenAsync(string userId, string tenantId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get tenant metadata cache
    /// </summary>
    Task<string?> GetTenantMetadataAsync(string tenantId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cache tenant metadata
    /// </summary>
    Task SetTenantMetadataAsync(string tenantId, string metadata, TimeSpan expiration, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Invalidate tenant metadata cache
    /// </summary>
    Task InvalidateTenantMetadataAsync(string tenantId, CancellationToken cancellationToken = default);
}

/// <summary>
/// In-memory implementation for development (no caching)
/// </summary>
public class InMemoryIdentityCacheService : IIdentityCacheService
{
    private readonly Dictionary<string, string> _cache = new();
    private readonly Dictionary<string, DateTime> _expirations = new();
    
    public Task<string?> GetTokenAsync(string userId, string tenantId, CancellationToken cancellationToken = default)
    {
        var key = $"token:{tenantId}:{userId}";
        return Task.FromResult(_cache.TryGetValue(key, out var value) ? value : null);
    }
    
    public Task SetTokenAsync(string userId, string tenantId, string token, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        var key = $"token:{tenantId}:{userId}";
        _cache[key] = token;
        _expirations[key] = DateTime.UtcNow + expiration;
        return Task.CompletedTask;
    }
    
    public Task InvalidateTokenAsync(string userId, string tenantId, CancellationToken cancellationToken = default)
    {
        var key = $"token:{tenantId}:{userId}";
        _cache.Remove(key);
        _expirations.Remove(key);
        return Task.CompletedTask;
    }
    
    public Task<string?> GetTenantMetadataAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var key = $"tenant:{tenantId}:metadata";
        return Task.FromResult(_cache.TryGetValue(key, out var value) ? value : null);
    }
    
    public Task SetTenantMetadataAsync(string tenantId, string metadata, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        var key = $"tenant:{tenantId}:metadata";
        _cache[key] = metadata;
        _expirations[key] = DateTime.UtcNow + expiration;
        return Task.CompletedTask;
    }
    
    public Task InvalidateTenantMetadataAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var key = $"tenant:{tenantId}:metadata";
        _cache.Remove(key);
        _expirations.Remove(key);
        return Task.CompletedTask;
    }
    
    public void CleanupExpired()
    {
        var now = DateTime.UtcNow;
        var expiredKeys = _expirations
            .Where(kvp => kvp.Value < now)
            .Select(kvp => kvp.Key)
            .ToList();
        
        foreach (var key in expiredKeys)
        {
            _cache.Remove(key);
            _expirations.Remove(key);
        }
    }
}