using NexoCommerceAI.Domain.Common;

namespace NexoCommerceAI.Domain.Events;

public sealed record PaymentProcessedIntegrationEvent(
    Guid OrderId,
    string OrderNumber,
    string TransactionId,
    decimal Amount,
    bool Success,
    DateTime ProcessedAt) : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
}