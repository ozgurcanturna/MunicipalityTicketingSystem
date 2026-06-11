namespace Journey.Telemetry.Api.Application.Contracts;

public sealed record CheckOutRequest(string CardId, string StopCode);