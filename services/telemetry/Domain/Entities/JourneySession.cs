using SharedKernel.Domain.Entities;

namespace Journey.Telemetry.Api.Domain.Entities;

public sealed class JourneySession : AggregateRoot
{
    private readonly List<JourneyCheckpoint> _checkpoints = [];

    public Guid TenantId { get; private set; }
    public string VehicleId { get; private set; } = string.Empty;
    public string RouteCode { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public int PassengerCount { get; private set; }
    public double CurrentLatitude { get; private set; }
    public double CurrentLongitude { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public IReadOnlyCollection<JourneyCheckpoint> Checkpoints => _checkpoints.AsReadOnly();

    private JourneySession()
    {
    }

    private JourneySession(Guid tenantId, string vehicleId, string routeCode, double latitude, double longitude)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("TenantId zorunludur.", nameof(tenantId));
        }

        if (string.IsNullOrWhiteSpace(vehicleId))
        {
            throw new ArgumentException("VehicleId zorunludur.", nameof(vehicleId));
        }

        if (string.IsNullOrWhiteSpace(routeCode))
        {
            throw new ArgumentException("RouteCode zorunludur.", nameof(routeCode));
        }

        TenantId = tenantId;
        VehicleId = vehicleId;
        RouteCode = routeCode;
        CurrentLatitude = latitude;
        CurrentLongitude = longitude;
        StartedAt = DateTime.UtcNow;
        RegisterCreated();
    }

    public static JourneySession Start(Guid tenantId, string vehicleId, string routeCode, double latitude, double longitude)
    {
        return new JourneySession(tenantId, vehicleId, routeCode, latitude, longitude);
    }

    public void UpdateLocation(double latitude, double longitude, string source)
    {
        EnsureActive();

        if (string.IsNullOrWhiteSpace(source))
        {
            throw new ArgumentException("Source zorunludur.", nameof(source));
        }

        CurrentLatitude = latitude;
        CurrentLongitude = longitude;
        _checkpoints.Add(JourneyCheckpoint.CreateLocation(Id, latitude, longitude, source));
        RegisterUpdated();
    }

    public void CheckIn(string cardId, string stopCode)
    {
        EnsureActive();
        ValidateCardAndStop(cardId, stopCode);

        PassengerCount++;
        _checkpoints.Add(JourneyCheckpoint.CreateCheckIn(Id, cardId, stopCode));
        RegisterUpdated();
    }

    public void CheckOut(string cardId, string stopCode)
    {
        EnsureActive();
        ValidateCardAndStop(cardId, stopCode);

        if (PassengerCount > 0)
        {
            PassengerCount--;
        }

        _checkpoints.Add(JourneyCheckpoint.CreateCheckOut(Id, cardId, stopCode));
        RegisterUpdated();
    }

    public void Complete()
    {
        EnsureActive();
        IsActive = false;
        EndedAt = DateTime.UtcNow;
        RegisterUpdated();
    }

    private void EnsureActive()
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Tamamlanmış yolculuk üzerinde işlem yapılamaz.");
        }
    }

    private static void ValidateCardAndStop(string cardId, string stopCode)
    {
        if (string.IsNullOrWhiteSpace(cardId))
        {
            throw new ArgumentException("CardId zorunludur.", nameof(cardId));
        }

        if (string.IsNullOrWhiteSpace(stopCode))
        {
            throw new ArgumentException("StopCode zorunludur.", nameof(stopCode));
        }
    }
}