using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace SharedKernel.Infrastructure.Caching;

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
