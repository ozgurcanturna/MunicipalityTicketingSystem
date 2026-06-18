using System;

namespace SharedKernel.Infrastructure.Caching;

/// <summary>
/// Interface for distributed caching service in Wallet service
/// </summary>
public interface IWalletCacheService
{
    /// <summary>
    /// Get wallet balance from cache
    /// </summary>
    Task<string?> GetWalletBalanceAsync(string walletId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Set wallet balance in cache
    /// </summary>
    Task SetWalletBalanceAsync(string walletId, string balanceJson, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Invalidate wallet balance cache
    /// </summary>
    Task InvalidateWalletBalanceAsync(string walletId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get transaction history cache
    /// </summary>
    Task<string?> GetTransactionsAsync(string walletId, int count, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Set transaction history in cache
    /// </summary>
    Task SetTransactionsAsync(string walletId, string transactionsJson, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Redis implementation of IWalletCacheService
/// </summary>
public sealed class RedisWalletCacheService : IWalletCacheService
{
    private readonly IDatabase _database;
    private readonly string _prefix;

    public RedisWalletCacheService(IConnectionMultiplexer redis, IConfiguration config)
    {
        _database = redis.GetDatabase();
        _prefix = config.GetSection("Redis:Prefix").Get<string>() ?? "muni:";
    }

    public async Task<string?> GetWalletBalanceAsync(string walletId, CancellationToken cancellationToken = default)
    {
        return await _database.StringGetAsync($"{_prefix}wallet:{walletId}:balance", cancellationToken);
    }

    public async Task SetWalletBalanceAsync(string walletId, string balanceJson, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var expireTime = expiration ?? TimeSpan.FromMinutes(5);
        await _database.StringSetAsync($"{_prefix}wallet:{walletId}:balance", balanceJson, expireTime, When.Always, cancellationToken);
    }

    public async Task InvalidateWalletBalanceAsync(string walletId, CancellationToken cancellationToken = default)
    {
        await _database.KeyDeleteAsync($"{_prefix}wallet:{walletId}:balance", cancellationToken);
    }

    public async Task<string?> GetTransactionsAsync(string walletId, int count, CancellationToken cancellationToken = default)
    {
        var key = $"{_prefix}wallet:{walletId}:transactions:{count}";
        return await _database.StringGetAsync(key, cancellationToken);
    }

    public async Task SetTransactionsAsync(string walletId, string transactionsJson, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var key = $"{_prefix}wallet:{walletId}:transactions:100"; // Cache last 100
        var expireTime = expiration ?? TimeSpan.FromMinutes(10);
        await _database.StringSetAsync(key, transactionsJson, expireTime, When.Always, cancellationToken);
    }
}