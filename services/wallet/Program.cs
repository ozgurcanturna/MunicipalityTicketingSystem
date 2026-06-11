using Microsoft.EntityFrameworkCore;
using SharedKernel.Infrastructure.DependencyInjection;
using SharedKernel.Infrastructure.MultiTenancy;
using Ticketing.Wallet.Api.Application.Contracts;
using Ticketing.Wallet.Api.Application.Repositories;
using Ticketing.Wallet.Api.Domain.Entities;
using Ticketing.Wallet.Api.Infrastructure.MultiTenancy;
using Ticketing.Wallet.Api.Infrastructure.Persistence;
using Ticketing.Wallet.Api.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ITenantProvider, HttpHeaderTenantProvider>();
builder.Services.AddSharedInfrastructure<WalletDbContext>(builder.Configuration);
builder.Services.AddScoped<IWalletRepository, WalletRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.Use(async (context, next) =>
{
	if (context.Request.Path == "/" || context.Request.Path.StartsWithSegments("/openapi"))
	{
		await next();
		return;
	}

	if (!context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdValues)
		|| string.IsNullOrWhiteSpace(tenantIdValues.FirstOrDefault()))
	{
		context.Response.StatusCode = StatusCodes.Status400BadRequest;
		await context.Response.WriteAsJsonAsync(new
		{
			message = "X-Tenant-Id header zorunludur."
		});
		return;
	}

	await next();
});

app.MapGet("/", () => "Ticketing Wallet API is running");

app.MapPost("/wallets", async (
	CreateWalletRequest request,
	IWalletRepository walletRepository,
	CancellationToken cancellationToken) =>
{
	var existing = await walletRepository.GetByTenantIdAsync(request.TenantId, cancellationToken);
	if (existing is not null)
	{
		return Results.Conflict("Bu tenant için wallet zaten mevcut.");
	}

	var wallet = WalletAccount.Create(request.TenantId);
	await walletRepository.AddAsync(wallet, cancellationToken);
	await walletRepository.SaveChangesAsync(cancellationToken);

	return Results.Created($"/wallets/{wallet.Id}", WalletResponse.FromDomain(wallet));
});

app.MapGet("/wallets/{id:guid}", async (
	Guid id,
	IWalletRepository walletRepository,
	CancellationToken cancellationToken) =>
{
	var wallet = await walletRepository.GetByIdAsync(id, cancellationToken);
	return wallet is null ? Results.NotFound() : Results.Ok(WalletResponse.FromDomain(wallet));
});

app.MapPost("/wallets/{id:guid}/topups", async (
	Guid id,
	TopUpWalletRequest request,
	IWalletRepository walletRepository,
	CancellationToken cancellationToken) =>
{
	var wallet = await walletRepository.GetByIdAsync(id, cancellationToken);
	if (wallet is null)
	{
		return Results.NotFound();
	}

	wallet.TopUp(request.Amount, request.Reference);
	walletRepository.Update(wallet);
	await walletRepository.SaveChangesAsync(cancellationToken);

	return Results.Ok(WalletResponse.FromDomain(wallet));
});

app.MapPost("/wallets/{id:guid}/spend", async (
	Guid id,
	SpendRequest request,
	IWalletRepository walletRepository,
	CancellationToken cancellationToken) =>
{
	var wallet = await walletRepository.GetByIdAsync(id, cancellationToken);
	if (wallet is null)
	{
		return Results.NotFound();
	}

	try
	{
		wallet.Spend(request.Amount, request.Reference);
	}
	catch (InvalidOperationException exception)
	{
		return Results.BadRequest(new { message = exception.Message });
	}

	walletRepository.Update(wallet);
	await walletRepository.SaveChangesAsync(cancellationToken);

	return Results.Ok(WalletResponse.FromDomain(wallet));
});

app.MapGet("/wallets/{id:guid}/transactions", async (
	Guid id,
	IWalletRepository walletRepository,
	CancellationToken cancellationToken) =>
{
	var wallet = await walletRepository.GetByIdAsync(id, cancellationToken);
	if (wallet is null)
	{
		return Results.NotFound();
	}

	var transactions = wallet.Transactions
		.Select(WalletTransactionResponse.FromDomain)
		.ToArray();

	return Results.Ok(transactions);
});

app.Run();
