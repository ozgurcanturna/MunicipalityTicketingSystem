using SharedKernel.Domain.Entities;

namespace Journey.Telemetry.Api.Domain.Entities;

public sealed class JourneyCheckpoint : Entity
{
    public Guid JourneySessionId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string? CardId { get; private set; }
    public string? StopCode { get; private set; }
    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }
    public string? Source { get; private set; }
    public DateTime OccurredAt { get; private set; }

    private JourneyCheckpoint()
    {
    }

    private JourneyCheckpoint(
        Guid journeySessionId,
        string eventType,
        string? cardId,
        string? stopCode,
        double? latitude,
        double? longitude,
        string? source)
    {
        JourneySessionId = journeySessionId;
        EventType = eventType;
        CardId = cardId;
        StopCode = stopCode;
        Latitude = latitude;
        Longitude = longitude;
        Source = source;
        OccurredAt = DateTime.UtcNow;
    }

    internal static JourneyCheckpoint CreateLocation(Guid journeySessionId, double latitude, double longitude, string source)
    {
        return new JourneyCheckpoint(
            journeySessionId,
            JourneyCheckpointType.LocationUpdated,
            null,
            null,
            latitude,
            longitude,
            source);
    }

    internal static JourneyCheckpoint CreateCheckIn(Guid journeySessionId, string cardId, string stopCode)
    {
        return new JourneyCheckpoint(
            journeySessionId,
            JourneyCheckpointType.CheckIn,
            cardId,
            stopCode,
            null,
            null,
            null);
    }

    internal static JourneyCheckpoint CreateCheckOut(Guid journeySessionId, string cardId, string stopCode)
    {
        return new JourneyCheckpoint(
            journeySessionId,
            JourneyCheckpointType.CheckOut,
            cardId,
            stopCode,
            null,
            null,
            null);
    }
}