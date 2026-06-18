using System;

namespace SharedKernel.Infrastructure.Caching;

/// <summary>
/// Interface for distributed caching service in Telemetry service
/// </summary>
public interface ITelemetryCacheService
{
    /// <summary>
    /// Get active journey for vehicle
    /// </summary>
    Task<string?> GetActiveJourneyAsync(string vehicleId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Set active journey in cache
    /// </summary>
    Task SetActiveJourneyAsync(string vehicleId, string journeyJson, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Invalidate active journey cache for vehicle
    /// </summary>
    Task InvalidateActiveJourneyAsync(string vehicleId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get journey checkpoints cache
    /// </summary>
    Task<string?> GetJourneyCheckpointsAsync(string journeyId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Set journey checkpoints in cache
    /// </summary>
    Task SetJourneyCheckpointsAsync(string journeyId, string checkpointsJson, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Redis implementation of ITelemetryCacheService
/// </summary>
public sealed class RedisTelemetryCacheService : ITelemetryCacheService
{
    private readonly IDatabase _database;
    private readonly string _prefix;

    public RedisTelemetryCacheService(IConnectionMultiplexer redis, IConfiguration config)
    {
        _database = redis.GetDatabase();
        _prefix = config.GetSection("Redis:Prefix").Get<string>() ?? "muni:";
    }

    public async Task<string?> GetActiveJourneyAsync(string vehicleId, CancellationToken cancellationToken = default)
    {
        return await _database.StringGetAsync($"{_prefix}journey:active:{vehicleId}", cancellationToken);
    }

    public async Task SetActiveJourneyAsync(string vehicleId, string journeyJson, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var expireTime = expiration ?? TimeSpan.FromMinutes(30);
        await _database.StringSetAsync($"{_prefix}journey:active:{vehicleId}", journeyJson, expireTime, When.Always, cancellationToken);
    }

    public async Task InvalidateActiveJourneyAsync(string vehicleId, CancellationToken cancellationToken = default)
    {
        await _database.KeyDeleteAsync($"{_prefix}journey:active:{vehicleId}", cancellationToken);
    }

    public async Task<string?> GetJourneyCheckpointsAsync(string journeyId, CancellationToken cancellationToken = default)
    {
        return await _database.StringGetAsync($"{_prefix}journey:{journeyId}:checkpoints", cancellationToken);
    }

    public async Task SetJourneyCheckpointsAsync(string journeyId, string checkpointsJson, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var expireTime = expiration ?? TimeSpan.FromHours(1);
        await _database.StringSetAsync($"{_prefix}journey:{journeyId}:checkpoints", checkpointsJson, expireTime, When.Always, cancellationToken);
    }
}