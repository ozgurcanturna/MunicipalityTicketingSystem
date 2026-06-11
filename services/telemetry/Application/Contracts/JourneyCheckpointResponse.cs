using Journey.Telemetry.Api.Domain.Entities;

namespace Journey.Telemetry.Api.Application.Contracts;

public sealed record JourneyCheckpointResponse(
    Guid Id,
    Guid JourneySessionId,
    string EventType,
    string? CardId,
    string? StopCode,
    double? Latitude,
    double? Longitude,
    string? Source,
    DateTime OccurredAt)
{
    public static JourneyCheckpointResponse FromDomain(JourneyCheckpoint checkpoint)
    {
        return new JourneyCheckpointResponse(
            checkpoint.Id,
            checkpoint.JourneySessionId,
            checkpoint.EventType,
            checkpoint.CardId,
            checkpoint.StopCode,
            checkpoint.Latitude,
            checkpoint.Longitude,
            checkpoint.Source,
            checkpoint.OccurredAt);
    }
}