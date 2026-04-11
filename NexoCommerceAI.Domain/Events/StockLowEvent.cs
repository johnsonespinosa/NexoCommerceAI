using NexoCommerceAI.Domain.Common;

namespace NexoCommerceAI.Domain.Events;

public sealed record StockLowEvent(
    Guid ProductId,
    string? ProductName,
    int CurrentStock,
    int Threshold) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}