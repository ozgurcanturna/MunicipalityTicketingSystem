using SharedKernel.Infrastructure.DependencyInjection;
using SharedKernel.Infrastructure.MultiTenancy;
using Journey.Telemetry.Api.Application.Contracts;
using Journey.Telemetry.Api.Application.Repositories;
using Journey.Telemetry.Api.Domain.Entities;
using Journey.Telemetry.Api.Infrastructure.MultiTenancy;
using Journey.Telemetry.Api.Infrastructure.Persistence;
using Journey.Telemetry.Api.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ITenantProvider, HttpHeaderTenantProvider>();
builder.Services.AddSharedInfrastructure<TelemetryDbContext>(builder.Configuration);
builder.Services.AddScoped<IJourneyRepository, JourneyRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.MapGet("/", () => "Journey Telemetry API is running");

app.MapPost("/journeys/start", async (
	StartJourneyRequest request,
	IJourneyRepository journeyRepository,
	CancellationToken cancellationToken) =>
{
	var existing = await journeyRepository.GetActiveByVehicleIdAsync(request.VehicleId, cancellationToken);
	if (existing is not null)
	{
		return Results.Conflict("Bu araç için aktif yolculuk zaten var.");
	}

	var journey = JourneySession.Start(
		request.TenantId,
		request.VehicleId.Trim(),
		request.RouteCode.Trim(),
		request.Latitude,
		request.Longitude);

	await journeyRepository.AddAsync(journey, cancellationToken);
	await journeyRepository.SaveChangesAsync(cancellationToken);

	return Results.Created($"/journeys/{journey.Id}", JourneySessionResponse.FromDomain(journey));
});

app.MapPost("/journeys/{id:guid}/locations", async (
	Guid id,
	UpdateLocationRequest request,
	IJourneyRepository journeyRepository,
	CancellationToken cancellationToken) =>
{
	var journey = await journeyRepository.GetByIdAsync(id, cancellationToken);
	if (journey is null)
	{
		return Results.NotFound();
	}

	journey.UpdateLocation(request.Latitude, request.Longitude, request.Source.Trim());
	journeyRepository.Update(journey);
	await journeyRepository.SaveChangesAsync(cancellationToken);

	return Results.Ok(JourneySessionResponse.FromDomain(journey));
});

app.MapPost("/journeys/{id:guid}/checkin", async (
	Guid id,
	CheckInRequest request,
	IJourneyRepository journeyRepository,
	CancellationToken cancellationToken) =>
{
	var journey = await journeyRepository.GetByIdAsync(id, cancellationToken);
	if (journey is null)
	{
		return Results.NotFound();
	}

	journey.CheckIn(request.CardId.Trim(), request.StopCode.Trim());
	journeyRepository.Update(journey);
	await journeyRepository.SaveChangesAsync(cancellationToken);

	return Results.Ok(JourneySessionResponse.FromDomain(journey));
});

app.MapPost("/journeys/{id:guid}/checkout", async (
	Guid id,
	CheckOutRequest request,
	IJourneyRepository journeyRepository,
	CancellationToken cancellationToken) =>
{
	var journey = await journeyRepository.GetByIdAsync(id, cancellationToken);
	if (journey is null)
	{
		return Results.NotFound();
	}

	journey.CheckOut(request.CardId.Trim(), request.StopCode.Trim());
	journeyRepository.Update(journey);
	await journeyRepository.SaveChangesAsync(cancellationToken);

	return Results.Ok(JourneySessionResponse.FromDomain(journey));
});

app.MapPost("/journeys/{id:guid}/complete", async (
	Guid id,
	IJourneyRepository journeyRepository,
	CancellationToken cancellationToken) =>
{
	var journey = await journeyRepository.GetByIdAsync(id, cancellationToken);
	if (journey is null)
	{
		return Results.NotFound();
	}

	journey.Complete();
	journeyRepository.Update(journey);
	await journeyRepository.SaveChangesAsync(cancellationToken);

	return Results.Ok(JourneySessionResponse.FromDomain(journey));
});

app.MapGet("/journeys/{id:guid}", async (
	Guid id,
	IJourneyRepository journeyRepository,
	CancellationToken cancellationToken) =>
{
	var journey = await journeyRepository.GetByIdAsync(id, cancellationToken);
	return journey is null ? Results.NotFound() : Results.Ok(JourneySessionResponse.FromDomain(journey));
});

app.MapGet("/journeys/active/{vehicleId}", async (
	string vehicleId,
	IJourneyRepository journeyRepository,
	CancellationToken cancellationToken) =>
{
	var journey = await journeyRepository.GetActiveByVehicleIdAsync(vehicleId.Trim(), cancellationToken);
	return journey is null ? Results.NotFound() : Results.Ok(JourneySessionResponse.FromDomain(journey));
});

app.Run();
