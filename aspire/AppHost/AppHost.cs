var builder = DistributedApplication.CreateBuilder(args);

// Infrastructure - Aspire uses container images with default settings
var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin();

var redis = builder.AddRedis("redis")
    .WithRedisInsight();

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();

// Services
var identity = builder.AddProject("identity", "../../services/identity/Tenant.Identity.Api.csproj")
    .WithEnvironment("ASPNETCORE_SERVICE_NAME", "identity-service")
    .WithReference(postgres)
    .WithReference(redis)
    .WaitFor(postgres)
    .WaitFor(redis);

var wallet = builder.AddProject("wallet", "../../services/wallet/Ticketing.Wallet.Api.csproj")
    .WithEnvironment("ASPNETCORE_SERVICE_NAME", "wallet-service")
    .WithReference(postgres)
    .WithReference(redis)
    .WaitFor(postgres)
    .WaitFor(redis);

var telemetry = builder.AddProject("telemetry", "../../services/telemetry/Journey.Telemetry.Api.csproj")
    .WithEnvironment("ASPNETCORE_SERVICE_NAME", "telemetry-service")
    .WithReference(postgres)
    .WithReference(redis)
    .WaitFor(postgres)
    .WaitFor(redis);

// Workers
var eventProcessor = builder.AddProject("event-processor", "../../workers/event-processor/Journey.EventProcessor.Worker.csproj")
    .WithReference(postgres)
    .WithReference(redis)
    .WaitFor(postgres)
    .WaitFor(redis);

// Gateway
var gateway = builder.AddProject("gateway", "../../gateway/ApiGateway.Yarp.csproj")
    .WithEnvironment("ASPNETCORE_SERVICE_NAME", "api-gateway")
    .WithReference(identity)
    .WithReference(wallet)
    .WithReference(telemetry)
    .WaitFor(identity)
    .WaitFor(wallet)
    .WaitFor(telemetry);

builder.Build().Run();