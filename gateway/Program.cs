using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;




using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using OpenTelemetry.Exporter;

var builder = WebApplication.CreateBuilder(args);

// Add OpenTelemetry for distributed tracing
if (bool.Parse(builder.Configuration["OpenTelemetry:Enabled"] ?? "false"))
{
    builder.Services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
            tracing
                .SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService(
                        serviceName: Environment.GetEnvironmentVariable("ASPNETCORE_SERVICE_NAME") ?? "api-gateway",
                        serviceVersion: builder.Configuration["App:Version"] ?? "1.0.0"))
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource("ApiGateway.Yarp")
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(builder.Configuration["OpenTelemetry:Traces:Endpoint"] ?? "http://otel-collector:4317");
                    options.Protocol = OtlpExportProtocol.Grpc;
                });
        });
}

builder.Services.AddOpenApi();
builder.Services.AddReverseProxy()
	.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

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

builder.Services.AddRateLimiter(options =>
{
	options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
	options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
	{
		var tenantId = httpContext.Request.Headers["X-Tenant-Id"].ToString();
		var partitionKey = string.IsNullOrWhiteSpace(tenantId)
			? $"ip:{httpContext.Connection.RemoteIpAddress}"
			: $"tenant:{tenantId}";

		return RateLimitPartition.GetFixedWindowLimiter(
			partitionKey,
			_ => new FixedWindowRateLimiterOptions
			{
				PermitLimit = 60,
				Window = TimeSpan.FromSeconds(60),
				QueueLimit = 0,
				QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
				AutoReplenishment = true
			});
	});
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseAuthentication();

// Correlation ID middleware for distributed tracing
app.Use(async (context, next) =>
{
	const string correlationHeader = "X-Correlation-Id";
	var correlationId = context.Request.Headers[correlationHeader].FirstOrDefault();

	if (string.IsNullOrWhiteSpace(correlationId))
	{
		correlationId = Guid.NewGuid().ToString("N");
		context.Request.Headers[correlationHeader] = correlationId;
	}

	context.Items["CorrelationId"] = correlationId;
	context.Response.Headers[correlationHeader] = correlationId;
	await next();
});

app.UseRateLimiter();

app.Use(async (context, next) =>
{
	if (!context.Request.Path.StartsWithSegments("/api"))
	{
		await next();
		return;
	}

	if (!context.Request.Headers.ContainsKey("X-Tenant-Id")
		&& !context.Request.Path.StartsWithSegments("/api/identity/auth/bootstrap"))
	{
		context.Response.StatusCode = StatusCodes.Status400BadRequest;
		await context.Response.WriteAsJsonAsync(new
		{
			message = "X-Tenant-Id header zorunludur."
		});
		return;
	}

	if (context.Request.Path.StartsWithSegments("/api/identity/auth/bootstrap")
		|| context.Request.Path.StartsWithSegments("/api/identity/auth/login"))
	{
		await next();
		return;
	}

	if (context.User.Identity?.IsAuthenticated != true)
	{
		context.Response.StatusCode = StatusCodes.Status401Unauthorized;
		await context.Response.WriteAsJsonAsync(new
		{
			message = "Bearer token zorunludur."
		});
		return;
	}

	var tenantHeader = context.Request.Headers["X-Tenant-Id"].ToString();
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

app.MapGet("/", () => "API Gateway (YARP) is running");
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.MapReverseProxy();

app.Run();
