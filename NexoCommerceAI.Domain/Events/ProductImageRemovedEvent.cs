using NexoCommerceAI.Domain.Common;

namespace NexoCommerceAI.Domain.Events;

public sealed record ProductImageRemovedEvent(
    Guid ProductId,
    string? ProductName,
    Guid ImageId,
    string ImageUrl,
    string PublicId) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}