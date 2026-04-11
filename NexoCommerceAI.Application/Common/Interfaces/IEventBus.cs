using MediatR;
using NexoCommerceAI.Domain.Common;

namespace NexoCommerceAI.Application.Common.Interfaces;

public interface IEventBus
{
    Task PublishAsync(IIntegrationEvent @event, CancellationToken cancellationToken = default);
    Task SubscribeAsync<T, THandler>()
        where T : IIntegrationEvent
        where THandler : INotificationHandler<T>;
}