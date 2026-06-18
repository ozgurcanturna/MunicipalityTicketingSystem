using System.Diagnostics;
using Journey.EventProcessor.Worker;
using Journey.EventProcessor.Worker.Configuration;
using Journey.EventProcessor.Worker.Events;
using Journey.EventProcessor.Worker.Processing;
using Journey.EventProcessor.Worker.Storage;


using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using OpenTelemetry.Exporter;

var builder = Host.CreateApplicationBuilder(args);

// Add OpenTelemetry for distributed tracing
if (bool.Parse(builder.Configuration["OpenTelemetry:Enabled"] ?? "false"))
{
    builder.Services.AddOpenTelemetry()
        .WithTracing(tracing =>
            tracing
                .SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService(
                        serviceName: Environment.GetEnvironmentVariable("ASPNETCORE_SERVICE_NAME") ?? "event-processor-worker",
                        serviceVersion: builder.Configuration["App:Version"] ?? "1.0.0"))
                .AddSource("Journey.EventProcessor.Worker")
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(builder.Configuration["OpenTelemetry:Traces:Endpoint"] ?? "http://otel-collector:4317");
                    options.Protocol = OtlpExportProtocol.Grpc;
                });
        });
}

builder.Services.Configure<EventProcessorOptions>(
	builder.Configuration.GetSection(EventProcessorOptions.SectionName));
builder.Services.Configure<EventBusOptions>(
	builder.Configuration.GetSection(EventBusOptions.SectionName));

builder.Services.AddSingleton<IEventQueue, InMemoryEventQueue>();
builder.Services.AddSingleton<IProcessedEventStore, InMemoryProcessedEventStore>();
builder.Services.AddSingleton<IDeadLetterStore, InMemoryDeadLetterStore>();
builder.Services.AddSingleton<IEventHandlerResolver, EventHandlerResolver>();

builder.Services.AddScoped<IIntegrationEventHandler, IdentityTenantCreatedEventHandler>();
builder.Services.AddScoped<IIntegrationEventHandler, WalletDebitedEventHandler>();
builder.Services.AddScoped<IIntegrationEventHandler, JourneyCompletedEventHandler>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
