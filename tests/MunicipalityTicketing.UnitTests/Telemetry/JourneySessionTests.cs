using Journey.Telemetry.Api.Domain.Entities;

namespace MunicipalityTicketing.UnitTests.Telemetry;

public sealed class JourneySessionTests
{
    [Fact]
    public void CheckIn_And_CheckOut_ShouldUpdatePassengerCount()
    {
        var journey = JourneySession.Start(Guid.NewGuid(), "BUS-1", "R-10", 39.90, 32.80);

        journey.CheckIn("CARD-1", "STOP-A");
        journey.CheckOut("CARD-1", "STOP-B");

        Assert.Equal(0, journey.PassengerCount);
        Assert.Equal(2, journey.Checkpoints.Count);
    }

    [Fact]
    public void UpdateLocation_WhenSourceEmpty_ShouldThrow()
    {
        var journey = JourneySession.Start(Guid.NewGuid(), "BUS-1", "R-10", 39.90, 32.80);

        var action = () => journey.UpdateLocation(39.91, 32.81, " ");

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Complete_WhenCalled_ShouldMakeJourneyInactive()
    {
        var journey = JourneySession.Start(Guid.NewGuid(), "BUS-1", "R-10", 39.90, 32.80);

        journey.Complete();

        Assert.False(journey.IsActive);
        Assert.NotNull(journey.EndedAt);
    }
}