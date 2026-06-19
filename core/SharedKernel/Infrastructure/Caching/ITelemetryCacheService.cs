using StackExchange.Redis;

namespace SharedKernel.Infrastructure.Caching;

public interface ITelemetryCacheService
{
    Task<bool> SetJourneyCacheAsync(Guid journeyId, string tenantId, CancellationToken cancellationToken);

    Task<JourneyCache?> GetJourneyCacheAsync(Guid journeyId, string tenantId, CancellationToken cancellationToken);

    Task InvalidateJourneyCacheAsync(Guid journeyId, string tenantId, CancellationToken cancellationToken);

    Task<bool> SetVehicleStatusCacheAsync(string vehicleId, string tenantId, string status, CancellationToken cancellationToken);

    Task<string?> GetVehicleStatusCacheAsync(string vehicleId, string tenantId, CancellationToken cancellationToken);

    Task InvalidateVehicleStatusCacheAsync(string vehicleId, string tenantId, CancellationToken cancellationToken);
}
