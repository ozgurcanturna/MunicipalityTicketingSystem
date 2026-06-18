using System.Text.Json;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace SharedKernel.Infrastructure.Caching;

public sealed class RedisWalletCacheService : IWalletCacheService
{
    private readonly IDatabase _database;
    private readonly string _prefix;

    public RedisWalletCacheService(IConnectionMultiplexer redis, IConfiguration configuration)
    {
        _database = redis.GetDatabase();
        _prefix = configuration.GetValue<string>("Redis:Prefix") ?? "muni:";
    }

    public async Task<bool> SetWalletCacheAsync(Guid walletId, string tenantId, CancellationToken cancellationToken)
    {
        var key = $"{_prefix}wallet:{tenantId}:{walletId}";
        var value = JsonSerializer.Serialize(new
        {
            WalletId = walletId,
            TenantId = tenantId,
            Balance = 0m,
            CachedAt = DateTime.UtcNow
        });

        await _database.StringSetAsync(key, value, TimeSpan.FromHours(1), when: When.NotExists);
        return true;
    }

    public async Task<WalletCache?> GetWalletCacheAsync(Guid walletId, string tenantId, CancellationToken cancellationToken)
    {
        var key = $"{_prefix}wallet:{tenantId}:{walletId}";
        var value = await _database.StringGetAsync(key, cancellationToken: cancellationToken);

        if (value.IsNullOrEmpty)
            return null;

        return JsonSerializer.Deserialize<WalletCache>(value.ToString());
    }

    public async Task InvalidateWalletCacheAsync(Guid walletId, string tenantId, CancellationToken cancellationToken)
    {
        var key = $"{_prefix}wallet:{tenantId}:{walletId}";
        await _database.KeyDeleteAsync(key, cancellationToken: cancellationToken);
    }

    public async Task<bool> SetBalanceCacheAsync(string tenantId, decimal balance, CancellationToken cancellationToken)
    {
        var key = $"{_prefix}wallet:balance:{tenantId}";
        var value = JsonSerializer.Serialize(new
        {
            TenantId = tenantId,
            Balance = balance,
            CachedAt = DateTime.UtcNow
        });

        await _database.StringSetAsync(key, value, TimeSpan.FromMinutes(5));
        return true;
    }

    public async Task<decimal?> GetBalanceCacheAsync(string tenantId, CancellationToken cancellationToken)
    {
        var key = $"{_prefix}wallet:balance:{tenantId}";
        var value = await _database.StringGetAsync(key, cancellationToken: cancellationToken);

        if (value.IsNullOrEmpty)
            return null;

        var cache = JsonSerializer.Deserialize<BalanceCache>(value.ToString());
        return cache?.Balance;
    }

    public async Task InvalidateBalanceCacheAsync(string tenantId, CancellationToken cancellationToken)
    {
        var key = $"{_prefix}wallet:balance:{tenantId}";
        await _database.KeyDeleteAsync(key, cancellationToken: cancellationToken);
    }
}

public sealed class WalletCache
{
    public Guid WalletId { get; set; }
    public Guid TenantId { get; set; }
    public decimal Balance { get; set; }
    public DateTime CachedAt { get; set; }
}

public sealed class BalanceCache
{
    public string TenantId { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public DateTime CachedAt { get; set; }
}
}

