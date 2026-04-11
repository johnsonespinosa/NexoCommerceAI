using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Domain.Common;

namespace NexoCommerceAI.Infrastructure.Services.EventBus;

public class InMemoryEventBus(IServiceProvider serviceProvider) : IEventBus
{
    private readonly Dictionary<string, List<Type>> _handlers = new();

    public async Task PublishAsync(IIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Publish(@event, cancellationToken);
    }
    
    public Task SubscribeAsync<T, THandler>()
        where T : IIntegrationEvent
        where THandler : INotificationHandler<T>
    {
        var eventName = typeof(T).Name;
        
        if (!_handlers.ContainsKey(eventName))
            _handlers[eventName] = [];
        
        _handlers[eventName].Add(typeof(THandler));
        
        return Task.CompletedTask;
    }
}