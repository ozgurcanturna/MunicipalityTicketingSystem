using Journey.Telemetry.Api.Domain.Entities;

namespace Journey.Telemetry.Api.Application.Contracts;

public sealed record JourneySessionResponse(
    Guid Id,
    Guid TenantId,
    string VehicleId,
    string RouteCode,
    bool IsActive,
    int PassengerCount,
    double CurrentLatitude,
    double CurrentLongitude,
    DateTime StartedAt,
    DateTime? EndedAt,
    IReadOnlyCollection<JourneyCheckpointResponse> Checkpoints)
{
    public static JourneySessionResponse FromDomain(JourneySession journey)
    {
        return new JourneySessionResponse(
            journey.Id,
            journey.TenantId,
            journey.VehicleId,
            journey.RouteCode,
            journey.IsActive,
            journey.PassengerCount,
            journey.CurrentLatitude,
            journey.CurrentLongitude,
            journey.StartedAt,
            journey.EndedAt,
            journey.Checkpoints.Select(JourneyCheckpointResponse.FromDomain).ToArray());
    }
}