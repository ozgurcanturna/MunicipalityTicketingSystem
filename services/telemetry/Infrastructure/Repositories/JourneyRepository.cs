using Microsoft.EntityFrameworkCore;
using SharedKernel.Infrastructure.Repositories;
using Journey.Telemetry.Api.Application.Repositories;
using Journey.Telemetry.Api.Domain.Entities;
using Journey.Telemetry.Api.Infrastructure.Persistence;

namespace Journey.Telemetry.Api.Infrastructure.Repositories;

public sealed class JourneyRepository : Repository<JourneySession>, IJourneyRepository
{
    public JourneyRepository(TelemetryDbContext dbContext)
        : base(dbContext)
    {
    }

    private TelemetryDbContext TelemetryDbContext => (TelemetryDbContext)DbContext;

    public new Task<JourneySession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return TelemetryDbContext.JourneySessions
            .Include(journey => journey.Checkpoints)
            .FirstOrDefaultAsync(journey => journey.Id == id, cancellationToken);
    }

    public Task<JourneySession?> GetActiveByVehicleIdAsync(string vehicleId, CancellationToken cancellationToken = default)
    {
        return TelemetryDbContext.JourneySessions
            .Include(journey => journey.Checkpoints)
            .FirstOrDefaultAsync(
                journey => journey.VehicleId == vehicleId && journey.IsActive,
                cancellationToken);
    }
}