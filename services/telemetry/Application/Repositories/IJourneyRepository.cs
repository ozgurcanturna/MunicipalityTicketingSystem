using SharedKernel.Domain.Repositories;
using Journey.Telemetry.Api.Domain.Entities;

namespace Journey.Telemetry.Api.Application.Repositories;

public interface IJourneyRepository : IRepository<JourneySession>
{
    Task<JourneySession?> GetActiveByVehicleIdAsync(string vehicleId, CancellationToken cancellationToken = default);
}