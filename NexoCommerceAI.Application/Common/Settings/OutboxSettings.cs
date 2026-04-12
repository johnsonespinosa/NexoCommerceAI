namespace NexoCommerceAI.Application.Common.Settings;

public class OutboxSettings
{
    public int IntervalSeconds { get; set; } = 5;
    public int BatchSize { get; set; } = 50;
    public int MaxRetryCount { get; set; } = 3;
    public bool Enabled { get; set; } = true;
}