namespace Journey.Telemetry.Api.Application.Contracts;

public sealed record CheckInRequest(string CardId, string StopCode);