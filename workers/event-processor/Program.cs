using Journey.EventProcessor.Worker;
using Journey.EventProcessor.Worker.Configuration;
using Journey.EventProcessor.Worker.Events;
using Journey.EventProcessor.Worker.Processing;
using Journey.EventProcessor.Worker.Storage;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<EventProcessorOptions>(
	builder.Configuration.GetSection(EventProcessorOptions.SectionName));

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
