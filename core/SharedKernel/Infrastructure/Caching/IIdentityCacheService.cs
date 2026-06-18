using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace SharedKernel.Infrastructure.Caching;

/// <summary>
/// Interface for distributed caching service in Identity service
/// </summary>
public interface IIdentityCacheService
{
    /// <summary>
    /// Cache tenant configuration data
    /// </summary>
    Task<string?> GetTenantConfigAsync(string tenantId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Set tenant configuration in cache
    /// </summary>
    Task SetTenantConfigAsync(string tenantId, string configJson, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cache JWT token refresh
    /// </summary>
    Task<string?> GetJwtTokenAsync(string userId, string tenantId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Set JWT token in cache
    /// </summary>
    Task SetJwtTokenAsync(string userId, string tenantId, string token, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Invalidate cache for a tenant
    /// </summary>
    Task InvalidateTenantCacheAsync(string tenantId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Redis implementation of IIdentityCacheService
/// </summary>
public sealed class RedisIdentityCacheService : IIdentityCacheService
{
    private readonly IDatabase _database;
    private readonly string _prefix;

    public RedisIdentityCacheService(IConnectionMultiplexer redis, IConfiguration config)
    {
        _database = redis.GetDatabase();
        _prefix = config.GetSection("Redis:Prefix").Get<string>() ?? "muni:";
    }

    public async Task<string?> GetTenantConfigAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        return await _database.StringGetAsync($"{_prefix}tenant:{tenantId}:config");
    }

    public async Task SetTenantConfigAsync(string tenantId, string configJson, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var expireTime = expiration ?? TimeSpan.FromMinutes(30);
        await _database.StringSetAsync($"{_prefix}tenant:{tenantId}:config", configJson, expireTime, When.Always);
    }

    public async Task<string?> GetJwtTokenAsync(string userId, string tenantId, CancellationToken cancellationToken = default)
    {
        return await _database.StringGetAsync($"{_prefix}jwt:{userId}:{tenantId}");
    }

    public async Task SetJwtTokenAsync(string userId, string tenantId, string token, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var expireTime = expiration ?? TimeSpan.FromMinutes(60);
        await _database.StringSetAsync($"{_prefix}jwt:{userId}:{tenantId}", token, expireTime, When.Always);
    }

    public async Task InvalidateTenantCacheAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var pattern = $"{_prefix}tenant:{tenantId}:*";
        await _database.KeyDeleteAsync(pattern);
    }
}