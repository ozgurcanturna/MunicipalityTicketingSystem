using System.Text.Json;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace SharedKernel.Infrastructure.Caching;

public sealed class RedisTelemetryCacheService : ITelemetryCacheService
{
    private readonly IDatabase _database;
    private readonly string _prefix;

    public RedisTelemetryCacheService(IConnectionMultiplexer redis, IConfiguration configuration)
    {
        _database = redis.GetDatabase();
        _prefix = configuration.GetValue<string>("Redis:Prefix") ?? "muni:";
    }

    public async Task<bool> SetJourneyCacheAsync(Guid journeyId, string tenantId, CancellationToken cancellationToken)
    {
        var key = $"{_prefix}journey:{tenantId}:{journeyId}";
        var value = JsonSerializer.Serialize(new
        {
            JourneyId = journeyId,
            TenantId = tenantId,
            VehicleId = string.Empty,
            Status = "Active",
            CachedAt = DateTime.UtcNow
        });

        await _database.StringSetAsync(key, value, TimeSpan.FromMinutes(10), when: When.NotExists);
        return true;
    }

    public async Task<JourneyCache?> GetJourneyCacheAsync(Guid journeyId, string tenantId, CancellationToken cancellationToken)
    {
        var key = $"{_prefix}journey:{tenantId}:{journeyId}";
        var value = await _database.StringGetAsync(key);

        if (value.IsNullOrEmpty)
            return null;

        return JsonSerializer.Deserialize<JourneyCache>(value.ToString());
    }

    public async Task InvalidateJourneyCacheAsync(Guid journeyId, string tenantId, CancellationToken cancellationToken)
    {
        var key = $"{_prefix}journey:{tenantId}:{journeyId}";
        await _database.KeyDeleteAsync(key);
    }

    public async Task<bool> SetVehicleStatusCacheAsync(string vehicleId, string tenantId, string status, CancellationToken cancellationToken)
    {
        var key = $"{_prefix}vehicle:{tenantId}:{vehicleId}";
        var value = JsonSerializer.Serialize(new
        {
            VehicleId = vehicleId,
            TenantId = tenantId,
            Status = status,
            LastUpdated = DateTime.UtcNow
        });

        await _database.StringSetAsync(key, value, TimeSpan.FromMinutes(5));
        return true;
    }

    public async Task<string?> GetVehicleStatusCacheAsync(string vehicleId, string tenantId, CancellationToken cancellationToken)
    {
        var key = $"{_prefix}vehicle:{tenantId}:{vehicleId}";
        var value = await _database.StringGetAsync(key);

        if (value.IsNullOrEmpty)
            return null;

        var cache = JsonSerializer.Deserialize<VehicleCache>(value.ToString());
        return cache?.Status;
    }

    public async Task InvalidateVehicleStatusCacheAsync(string vehicleId, string tenantId, CancellationToken cancellationToken)
    {
        var key = $"{_prefix}vehicle:{tenantId}:{vehicleId}";
        await _database.KeyDeleteAsync(key);
    }
}

public sealed class JourneyCache
{
    public Guid JourneyId { get; set; }
    public Guid TenantId { get; set; }
    public string VehicleId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CachedAt { get; set; }
}

public sealed class VehicleCache
{
    public string VehicleId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
}
