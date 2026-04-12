using NexoCommerceAI.Domain.Common;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Domain.Events;

public sealed record CartAbandonedEvent(
    Guid UserId,
    decimal TotalAmount,
    int TotalItems,
    IReadOnlyList<CartItem> Items) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}