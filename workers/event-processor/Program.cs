var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

Console.WriteLine("Journey EventProcessor Worker is running");
builder.Build().Run();
