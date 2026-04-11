using MediatR;

namespace NexoCommerceAI.Domain.Common;

public interface IDomainEvent : INotification
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
}