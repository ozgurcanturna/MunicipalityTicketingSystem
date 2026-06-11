namespace Journey.Telemetry.Api.Application.Contracts;

public sealed record StartJourneyRequest(
    Guid TenantId,
    string VehicleId,
    string RouteCode,
    double Latitude,
    double Longitude);