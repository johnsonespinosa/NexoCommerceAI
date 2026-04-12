// Domain/Events/Orders/InventoryReservedIntegrationEvent.cs

using NexoCommerceAI.Domain.Common;

namespace NexoCommerceAI.Domain.Events;

public sealed record InventoryReservedIntegrationEvent(
    Guid OrderId,
    string OrderNumber,
    IReadOnlyList<ReservedItem> Items,
    DateTime ReservedAt) : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
}

public record ReservedItem(Guid ProductId, string ProductName, int Quantity);