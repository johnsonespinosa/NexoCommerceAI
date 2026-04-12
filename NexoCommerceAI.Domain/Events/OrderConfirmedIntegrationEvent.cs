using NexoCommerceAI.Domain.Common;

namespace NexoCommerceAI.Domain.Events;

public sealed record OrderConfirmedIntegrationEvent(
    Guid OrderId,
    string OrderNumber,
    Guid UserId,
    decimal TotalAmount,
    DateTime ConfirmedAt) : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
}