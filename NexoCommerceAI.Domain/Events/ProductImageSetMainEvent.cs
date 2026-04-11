using NexoCommerceAI.Domain.Common;

namespace NexoCommerceAI.Domain.Events;

public sealed record ProductImageSetMainEvent(
    Guid ProductId,
    string? ProductName,
    Guid ImageId,
    string ImageUrl,
    bool IsMain) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}