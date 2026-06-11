using Journey.EventProcessor.Worker.Events;
using Journey.EventProcessor.Worker.Processing;

namespace MunicipalityTicketing.IntegrationTests.EventProcessor;

public sealed class EventHandlerResolverTests
{
    [Fact]
    public void Resolve_ShouldFindHandler_CaseInsensitive()
    {
        var resolver = new EventHandlerResolver(new[] { new FakeHandler() });

        var handler = resolver.Resolve("fake.event");

        Assert.NotNull(handler);
        Assert.Equal("Fake.Event", handler!.EventType);
    }

    [Fact]
    public void Resolve_WhenUnknownEventType_ShouldReturnNull()
    {
        var resolver = new EventHandlerResolver(new[] { new FakeHandler() });

        var handler = resolver.Resolve("unknown.event");

        Assert.Null(handler);
    }

    private sealed class FakeHandler : IIntegrationEventHandler
    {
        public string EventType => "Fake.Event";

        public Task HandleAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}