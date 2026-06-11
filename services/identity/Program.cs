using Microsoft.EntityFrameworkCore;
using SharedKernel.Infrastructure.DependencyInjection;
using SharedKernel.Infrastructure.MultiTenancy;
using Tenant.Identity.Api.Application.Contracts;
using Tenant.Identity.Api.Application.Repositories;
using Tenant.Identity.Api.Domain.Entities;
using Tenant.Identity.Api.Infrastructure.MultiTenancy;
using Tenant.Identity.Api.Infrastructure.Persistence;
using Tenant.Identity.Api.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ITenantProvider, HttpHeaderTenantProvider>();
builder.Services.AddSharedInfrastructure<IdentityDbContext>(builder.Configuration);
builder.Services.AddScoped<ITenantRepository, TenantRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.MapGet("/", () => "Tenant Identity API is running");

app.MapPost("/tenants", async (
	CreateTenantRequest request,
	ITenantRepository tenantRepository,
	CancellationToken cancellationToken) =>
{
	var tenant = MunicipalityTenant.Create(request.Name.Trim());

	await tenantRepository.AddAsync(tenant, cancellationToken);
	await tenantRepository.SaveChangesAsync(cancellationToken);

	return Results.Created($"/tenants/{tenant.Id}", TenantResponse.FromDomain(tenant));
});

app.MapGet("/tenants/{id:guid}", async (
	Guid id,
	ITenantRepository tenantRepository,
	CancellationToken cancellationToken) =>
{
	var tenant = await tenantRepository.GetByIdAsync(id, cancellationToken);

	return tenant is null
		? Results.NotFound()
		: Results.Ok(TenantResponse.FromDomain(tenant));
});

app.MapPost("/tenants/{id:guid}/users", async (
	Guid id,
	AddUserRequest request,
	ITenantRepository tenantRepository,
	CancellationToken cancellationToken) =>
{
	var tenant = await tenantRepository.GetByIdAsync(id, cancellationToken);

	if (tenant is null)
	{
		return Results.NotFound();
	}

	var user = tenant.AddUser(request.Email.Trim(), request.FullName.Trim());
	tenantRepository.Update(tenant);
	await tenantRepository.SaveChangesAsync(cancellationToken);

	return Results.Ok(UserResponse.FromDomain(user));
});

app.Run();
