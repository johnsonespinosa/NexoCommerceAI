namespace NexoCommerceAI.Domain.Entities;

public class ProcessedWebhookEvent
{
    public Guid Id { get; set; }
    public string EventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
    public string? OrderId { get; set; }
}