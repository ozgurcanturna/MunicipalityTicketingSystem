using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SharedKernel.Infrastructure.DependencyInjection;
using SharedKernel.Infrastructure.MultiTenancy;
using Tenant.Identity.Api.Application.Contracts;
using Tenant.Identity.Api.Application.Repositories;
using Tenant.Identity.Api.Domain.Entities;
using Tenant.Identity.Api.Infrastructure.Authentication;
using Tenant.Identity.Api.Infrastructure.MultiTenancy;
using Tenant.Identity.Api.Infrastructure.Persistence;
using Tenant.Identity.Api.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
var signingKey = Encoding.UTF8.GetBytes(jwtOptions.SecretKey);

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
			ValidIssuer = jwtOptions.Issuer,
			ValidAudience = jwtOptions.Audience,
			IssuerSigningKey = new SymmetricSecurityKey(signingKey),
			ClockSkew = TimeSpan.FromMinutes(1)
		};
	});

builder.Services.AddAuthorization(options =>
{
	options.AddPolicy("TenantAdmin", policy => policy.RequireRole("ADMIN"));
	options.AddPolicy("AuthenticatedUser", policy => policy.RequireAuthenticatedUser());
});

builder.Services.AddScoped<ITenantProvider, HttpHeaderTenantProvider>();
builder.Services.AddSharedInfrastructure<IdentityDbContext>(builder.Configuration);
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddSingleton<IPasswordHasher, Sha256PasswordHasher>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseAuthentication();

app.Use(async (context, next) =>
{
	if (context.Request.Path == "/"
		|| context.Request.Path.StartsWithSegments("/openapi")
		|| context.Request.Path.StartsWithSegments("/auth/bootstrap")
		|| context.Request.Path.StartsWithSegments("/auth/login"))
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

	if (context.User.Identity?.IsAuthenticated == true)
	{
		var tenantHeader = tenantIdValues.FirstOrDefault()?.Trim();
		var tenantClaim = context.User.FindFirst("tenant_id")?.Value;

		if (!string.Equals(tenantHeader, tenantClaim, StringComparison.OrdinalIgnoreCase))
		{
			context.Response.StatusCode = StatusCodes.Status403Forbidden;
			await context.Response.WriteAsJsonAsync(new
			{
				message = "Token tenant bilgisi ile X-Tenant-Id uyusmuyor."
			});
			return;
		}
	}

	await next();
});

app.UseAuthorization();

app.MapGet("/", () => "Tenant Identity API is running");

app.MapPost("/auth/bootstrap", async (
	BootstrapTenantRequest request,
	IPasswordHasher passwordHasher,
	IJwtTokenService jwtTokenService,
	ITenantRepository tenantRepository,
	CancellationToken cancellationToken) =>
{
	var existingTenants = await tenantRepository.GetAllAsync(cancellationToken);
	if (existingTenants.Count > 0)
	{
		return Results.Conflict(new
		{
			message = "Bootstrap sadece ilk tenant olusturulurken kullanilabilir."
		});
	}

	var tenant = MunicipalityTenant.Create(request.TenantName.Trim());
	var adminUser = tenant.AddUser(
		request.AdminEmail.Trim(),
		request.AdminFullName.Trim(),
		passwordHasher.Hash(request.AdminPassword.Trim()),
		"ADMIN");

	await tenantRepository.AddAsync(tenant, cancellationToken);
	await tenantRepository.SaveChangesAsync(cancellationToken);

	adminUser.RecordLogin();
	tenantRepository.Update(tenant);
	await tenantRepository.SaveChangesAsync(cancellationToken);

	return Results.Created("/auth/bootstrap", new
	{
		tenant = TenantResponse.FromDomain(tenant),
		admin = UserResponse.FromDomain(adminUser),
		token = jwtTokenService.CreateToken(tenant, adminUser)
	});
});

app.MapPost("/tenants", async (
	CreateTenantRequest request,
	ITenantRepository tenantRepository,
	CancellationToken cancellationToken) =>
{
	var tenant = MunicipalityTenant.Create(request.Name.Trim());

	await tenantRepository.AddAsync(tenant, cancellationToken);
	await tenantRepository.SaveChangesAsync(cancellationToken);

	return Results.Created($"/tenants/{tenant.Id}", TenantResponse.FromDomain(tenant));
}).RequireAuthorization("TenantAdmin");

app.MapGet("/tenants/{id:guid}", async (
	Guid id,
	ITenantRepository tenantRepository,
	CancellationToken cancellationToken) =>
{
	var tenant = await tenantRepository.GetByIdAsync(id, cancellationToken);

	return tenant is null
		? Results.NotFound()
		: Results.Ok(TenantResponse.FromDomain(tenant));
}).RequireAuthorization("AuthenticatedUser");

app.MapPost("/tenants/{id:guid}/users", async (
	Guid id,
	AddUserRequest request,
	IPasswordHasher passwordHasher,
	ITenantRepository tenantRepository,
	CancellationToken cancellationToken) =>
{
	var tenant = await tenantRepository.GetByIdAsync(id, cancellationToken);

	if (tenant is null)
	{
		return Results.NotFound();
	}

	var user = tenant.AddUser(
		request.Email.Trim(),
		request.FullName.Trim(),
		passwordHasher.Hash(request.Password.Trim()),
		request.Role.Trim());
	tenantRepository.Update(tenant);
	await tenantRepository.SaveChangesAsync(cancellationToken);

	return Results.Ok(UserResponse.FromDomain(user));
}).RequireAuthorization("TenantAdmin");

app.MapPost("/auth/login", async (
	LoginRequest request,
	IPasswordHasher passwordHasher,
	IJwtTokenService jwtTokenService,
	ITenantRepository tenantRepository,
	CancellationToken cancellationToken) =>
{
	var tenant = await tenantRepository.GetByUserEmailAsync(request.TenantId, request.Email.Trim(), cancellationToken);
	if (tenant is null)
	{
		return Results.Unauthorized();
	}

	var user = tenant.Users.FirstOrDefault(candidate =>
		string.Equals(candidate.Email, request.Email.Trim(), StringComparison.OrdinalIgnoreCase));

	if (user is null || !user.IsActive || !passwordHasher.Verify(request.Password.Trim(), user.PasswordHash))
	{
		return Results.Unauthorized();
	}

	user.RecordLogin();
	tenantRepository.Update(tenant);
	await tenantRepository.SaveChangesAsync(cancellationToken);

	return Results.Ok(jwtTokenService.CreateToken(tenant, user));
});

app.MapGet("/auth/me", (
	ClaimsPrincipal user) => Results.Ok(new
	{
		userId = user.FindFirstValue(ClaimTypes.NameIdentifier),
		email = user.FindFirstValue(ClaimTypes.Email) ?? user.FindFirstValue("email"),
		name = user.Identity?.Name,
		role = user.FindFirstValue(ClaimTypes.Role),
		tenantId = user.FindFirstValue("tenant_id")
	})).RequireAuthorization("AuthenticatedUser");

app.Run();
