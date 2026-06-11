namespace Journey.EventProcessor.Worker.Configuration;

public sealed class EventProcessorOptions
{
    public const string SectionName = "EventProcessor";

    public int MaxRetryCount { get; init; } = 3;
    public int BaseRetryDelayMs { get; init; } = 250;
    public bool SeedDemoEvents { get; init; } = true;
}