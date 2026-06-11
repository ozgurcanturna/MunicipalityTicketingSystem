namespace Journey.Telemetry.Api.Application.Contracts;

public sealed record UpdateLocationRequest(double Latitude, double Longitude, string Source);