using NexoCommerceAI.Domain.Common;

namespace NexoCommerceAI.Domain.Events;

public sealed record ProductCreatedIntegrationEvent(
    Guid ProductId,
    string ProductName,
    decimal Price,
    int Stock) : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
}