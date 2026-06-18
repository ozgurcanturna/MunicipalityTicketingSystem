using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SharedKernel.Domain.Common;
using StackExchange.Redis;

namespace SharedKernel.Infrastructure.Caching;

public sealed class RedisIdentityCacheService : IIdentityCacheService
{
    private readonly IDatabase _database;
    private readonly string _prefix;

    public RedisIdentityCacheService(IConnectionMultiplexer redis, IConfiguration configuration)
    {
        _database = redis.GetDatabase();
        _prefix = configuration.GetValue<string>("Redis:Prefix") ?? "muni:";
    }

    public async Task<bool> SetUserCacheAsync(Guid userId, string tenantId, CancellationToken cancellationToken)
    {
        var key = $"{_prefix}user:{tenantId}:{userId}";
        var value = JsonSerializer.Serialize(new
        {
            UserId = userId,
            TenantId = tenantId,
            CachedAt = DateTime.UtcNow
        });

        await _database.StringSetAsync(key, value, TimeSpan.FromHours(1), when: When.NotExists);
        return true;
    }

    public async Task<UserCache?> GetUserCacheAsync(Guid userId, string tenantId, CancellationToken cancellationToken)
    {
        var key = $"{_prefix}user:{tenantId}:{userId}";
        var value = await _database.StringGetAsync(key, cancellationToken: cancellationToken);

        if (value.IsNullOrEmpty)
            return null;

        return JsonSerializer.Deserialize<UserCache>(value.ToString());
    }

    public async Task InvalidateUserCacheAsync(Guid userId, string tenantId, CancellationToken cancellationToken)
    {
        var key = $"{_prefix}user:{tenantId}:{userId}";
        await _database.KeyDeleteAsync(key, cancellationToken: cancellationToken);
    }

    public async Task<bool> SetTenantCacheAsync(Guid tenantId, Tenant tenant, CancellationToken cancellationToken)
    {
        var key = $"{_prefix}tenant:{tenantId}";
        var value = JsonSerializer.Serialize(new
        {
            Id = tenant.Id,
            Name = tenant.Name,
            CreatedAt = tenant.CreatedAt,
            Status = tenant.Status
        });

        await _database.StringSetAsync(key, value, TimeSpan.FromHours(6), when: When.NotExists);
        return true;
    }

    public async Task<Tenant?> GetTenantCacheAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var key = $"{_prefix}tenant:{tenantId}";
        var value = await _database.StringGetAsync(key, cancellationToken: cancellationToken);

        if (value.IsNullOrEmpty)
            return null;

        return JsonSerializer.Deserialize<Tenant>(value.ToString());
    }

    public async Task InvalidateTenantCacheAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var key = $"{_prefix}tenant:{tenantId}";
        await _database.KeyDeleteAsync(key, cancellationToken: cancellationToken);
    }
}

public sealed class UserCache
{
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }
    public DateTime CachedAt { get; set; }
}

public sealed class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}
}
