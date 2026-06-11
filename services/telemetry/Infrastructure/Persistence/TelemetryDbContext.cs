using Microsoft.EntityFrameworkCore;
using SharedKernel.Infrastructure.Persistence;
using Journey.Telemetry.Api.Domain.Entities;

namespace Journey.Telemetry.Api.Infrastructure.Persistence;

public sealed class TelemetryDbContext : AppDbContext
{
    public TelemetryDbContext(DbContextOptions<TelemetryDbContext> options)
        : base(options)
    {
    }

    public DbSet<JourneySession> JourneySessions => Set<JourneySession>();
    public DbSet<JourneyCheckpoint> JourneyCheckpoints => Set<JourneyCheckpoint>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<JourneySession>(entity =>
        {
            entity.ToTable("JourneySessions");
            entity.HasKey(journey => journey.Id);
            entity.Property(journey => journey.VehicleId).HasMaxLength(64).IsRequired();
            entity.Property(journey => journey.RouteCode).HasMaxLength(64).IsRequired();
            entity.HasIndex(journey => new { journey.TenantId, journey.VehicleId, journey.IsActive });

            entity
                .HasMany(journey => journey.Checkpoints)
                .WithOne()
                .HasForeignKey(checkpoint => checkpoint.JourneySessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<JourneyCheckpoint>(entity =>
        {
            entity.ToTable("JourneyCheckpoints");
            entity.HasKey(checkpoint => checkpoint.Id);
            entity.Property(checkpoint => checkpoint.EventType).HasMaxLength(32).IsRequired();
            entity.Property(checkpoint => checkpoint.CardId).HasMaxLength(128);
            entity.Property(checkpoint => checkpoint.StopCode).HasMaxLength(64);
            entity.Property(checkpoint => checkpoint.Source).HasMaxLength(64);
            entity.HasIndex(checkpoint => checkpoint.JourneySessionId);
            entity.HasIndex(checkpoint => checkpoint.OccurredAt);
        });
    }
}