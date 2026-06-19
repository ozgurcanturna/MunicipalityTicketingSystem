using System;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SharedKernel.Infrastructure.DependencyInjection;
using SharedKernel.Infrastructure.Caching;
using SharedKernel.Infrastructure.MultiTenancy;
using StackExchange.Redis;
using Ticketing.Wallet.Api.Application.Contracts;
using Ticketing.Wallet.Api.Application.Repositories;
using Ticketing.Wallet.Api.Domain.Entities;
using Ticketing.Wallet.Api.Infrastructure.MultiTenancy;
using Ticketing.Wallet.Api.Infrastructure.Persistence;
using Ticketing.Wallet.Api.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add ServiceDefaults (Aspire integration)
builder.AddServiceDefaults();

builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "MunicipalityTicketing.Identity";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "MunicipalityTicketing.Clients";
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? "super-secret-development-key-change-me-12345";

builder.Services
	.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateIssuerSigningKey = true,
			ValidateLifetime = true,
			ValidIssuer = jwtIssuer,
			ValidAudience = jwtAudience,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
			ClockSkew = TimeSpan.FromMinutes(1)
		};
	});

builder.Services.AddAuthorization(options =>
{
	options.AddPolicy("WalletAdmin", policy => policy.RequireRole("ADMIN"));
	options.AddPolicy("WalletUser", policy => policy.RequireRole("ADMIN", "USER"));
});

if (bool.Parse(builder.Configuration["Redis:Enabled"] ?? "false"))
{
    var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
    
    builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    {
        var config = ConfigurationOptions.Parse(redisConnectionString);
        config.AbortOnConnectFail = false;
        return ConnectionMultiplexer.Connect(config);
    });
    
    builder.Services.AddScoped<IWalletCacheService, RedisWalletCacheService>();
}

builder.Services.AddScoped<ITenantProvider, HttpHeaderTenantProvider>();
builder.Services.AddSharedInfrastructure<WalletDbContext>(builder.Configuration);
builder.Services.AddScoped<IWalletRepository, WalletRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseAuthentication();

// Add correlation ID middleware for distributed tracing
app.Use(async (context, next) =>
{
	var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault();
	if (string.IsNullOrWhiteSpace(correlationId))
	{
		correlationId = Guid.NewGuid().ToString("N");
		context.Request.Headers["X-Correlation-Id"] = correlationId;
	}
	context.Items["CorrelationId"] = correlationId;
	await next();
});

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

	if (!context.User.Identity?.IsAuthenticated ?? true)
	{
		context.Response.StatusCode = StatusCodes.Status401Unauthorized;
		return;
	}

	var tenantHeader = tenantIdValues.FirstOrDefault()?.Trim();
	var tenantClaim = context.User.FindFirstValue("tenant_id");
	if (!string.Equals(tenantHeader, tenantClaim, StringComparison.OrdinalIgnoreCase))
	{
		context.Response.StatusCode = StatusCodes.Status403Forbidden;
		await context.Response.WriteAsJsonAsync(new
		{
			message = "Token tenant bilgisi ile X-Tenant-Id uyusmuyor."
		});
		return;
	}

	await next();
});

app.UseAuthorization();

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
}).RequireAuthorization("WalletAdmin");

app.MapGet("/wallets/{id:guid}", async (
	Guid id,
	IWalletRepository walletRepository,
	CancellationToken cancellationToken) =>
{
	var wallet = await walletRepository.GetByIdAsync(id, cancellationToken);
	return wallet is null ? Results.NotFound() : Results.Ok(WalletResponse.FromDomain(wallet));
}).RequireAuthorization("WalletUser");

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
}).RequireAuthorization("WalletAdmin");

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
}).RequireAuthorization("WalletUser");

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
}).RequireAuthorization("WalletUser");

// Map health check endpoints (via ServiceDefaults)
app.MapDefaultEndpoints();

app.Run();
