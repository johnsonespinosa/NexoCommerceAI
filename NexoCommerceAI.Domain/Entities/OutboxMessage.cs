using System.Text.Json.Serialization;

namespace NexoCommerceAI.Domain.Entities;

public class OutboxMessage
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime OccurredOn { get; set; }
    public DateTime? ProcessedOn { get; set; }
    public string? Error { get; set; }
    public int RetryCount { get; set; }
    
    // Nuevas propiedades para mejor trazabilidad
    public string? AggregateId { get; set; }
    public string? UserId { get; set; }
    public string? CorrelationId { get; set; }
    
    [JsonIgnore]
    public bool IsProcessed => ProcessedOn.HasValue;
    
    public bool ShouldRetry => RetryCount < 3 && !IsProcessed;
}