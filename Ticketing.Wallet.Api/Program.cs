var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => "Ticketing Wallet API is running");

app.Run();
