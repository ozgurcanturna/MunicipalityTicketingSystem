using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddReverseProxy()
	.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

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

app.Use(async (context, next) =>
{
	const string correlationHeader = "X-Correlation-Id";
	var correlationId = context.Request.Headers[correlationHeader].FirstOrDefault();

	if (string.IsNullOrWhiteSpace(correlationId))
	{
		correlationId = Guid.NewGuid().ToString("N");
		context.Request.Headers[correlationHeader] = correlationId;
	}

	context.Response.Headers[correlationHeader] = correlationId;
	await next();
});

app.UseRateLimiter();

app.Use(async (context, next) =>
{
	if (context.Request.Path.StartsWithSegments("/api")
		&& !context.Request.Headers.ContainsKey("X-Tenant-Id"))
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

app.MapGet("/", () => "API Gateway (YARP) is running");
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.MapReverseProxy();

app.Run();
