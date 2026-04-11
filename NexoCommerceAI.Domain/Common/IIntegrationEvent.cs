using MediatR;

namespace NexoCommerceAI.Domain.Common;

public interface IIntegrationEvent : INotification
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
    string EventType { get; }
}