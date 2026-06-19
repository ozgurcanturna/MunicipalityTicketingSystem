using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SharedKernel.Infrastructure.Persistence;
using Journey.Telemetry.Api.Domain.Entities;
using Journey.Telemetry.Api.Infrastructure.Persistence;

namespace Journey.Telemetry.Api.Infrastructure.Seeding;

public sealed class TelemetryDatabaseSeeder
{
    private static readonly Guid BursaTenantId = Guid.Parse("7f4c8c0f-1d7b-4d52-8a4d-000000000001");

    private readonly IConfiguration _configuration;
    private readonly ILogger<TelemetryDatabaseSeeder> _logger;

    public TelemetryDatabaseSeeder(IConfiguration configuration, ILogger<TelemetryDatabaseSeeder> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var connectionString = _configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:Default ayarı zorunludur.");
        }

        PostgresDatabaseInitializer.EnsureDatabaseExists(connectionString);

        var options = new DbContextOptionsBuilder<TelemetryDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        await using var dbContext = new TelemetryDbContext(options);

        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        // Create demo journey for Bursa tenant if not exists
        var existingJourney = await dbContext.JourneySessions
            .FirstOrDefaultAsync(j => j.TenantId == BursaTenantId && j.IsActive, cancellationToken);

        if (existingJourney is null)
        {
            // Demo active journey
            var journey = JourneySession.Start(
                BursaTenantId,
                "BUS-01",
                "Bursa-Kadıköy",
                40.1950,
                29.0610);

            // Simulate some passengers
            journey.CheckIn("CARD-001", "IST-01");
            journey.UpdateLocation(40.2000, 29.0650, "GPS");

            dbContext.JourneySessions.Add(journey);

            _logger.LogInformation("Seed journey created for Bursa tenant: {JourneyId}", journey.Id);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}