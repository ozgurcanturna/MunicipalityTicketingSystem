var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => "API Gateway (YARP) is running");

app.Run();
