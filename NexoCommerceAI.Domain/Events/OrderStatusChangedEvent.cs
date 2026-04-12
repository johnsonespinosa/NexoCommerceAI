using NexoCommerceAI.Domain.Common;
using NexoCommerceAI.Domain.Enums;

namespace NexoCommerceAI.Domain.Events;

public sealed record OrderStatusChangedEvent(
    Guid OrderId,
    string OrderNumber,
    OrderStatus OldStatus,
    OrderStatus NewStatus) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}