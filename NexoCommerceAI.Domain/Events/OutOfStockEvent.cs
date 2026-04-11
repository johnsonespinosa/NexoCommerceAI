using NexoCommerceAI.Domain.Common;

namespace NexoCommerceAI.Domain.Events;

public sealed record OutOfStockEvent(
    Guid ProductId,
    string? ProductName) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}