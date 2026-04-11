using NexoCommerceAI.Domain.Common;

namespace NexoCommerceAI.Domain.Events;

public sealed record StockRestockedEvent(
    Guid ProductId,
    string? ProductName,
    int PreviousStock,
    int NewStock,
    int QuantityAdded) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}