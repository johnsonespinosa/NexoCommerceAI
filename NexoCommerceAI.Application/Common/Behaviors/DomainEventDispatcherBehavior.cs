using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Domain.Common;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Common.Behaviors;

public class DomainEventDispatcherBehavior<TRequest, TResponse>(
    IApplicationDbContext dbContext,
    IMediator mediator,
    IOutboxRepository outboxRepository,
    ILogger<DomainEventDispatcherBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();
        
        // Obtener entidades con eventos de dominio
        var entitiesWithEvents = dbContext.ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Count != 0 || e.Entity.IntegrationEvents.Count != 0)
            .Select(e => e.Entity)
            .ToList();
        
        if (entitiesWithEvents.Count == 0)
            return response;
        
        // 1. Primero, publicar eventos de dominio (dentro de la transacción actual)
        var domainEvents = entitiesWithEvents.SelectMany(e => e.DomainEvents).ToList();
        
        foreach (var domainEvent in domainEvents)
        {
            logger.LogDebug("Publishing domain event: {EventType}", domainEvent.GetType().Name);
            await mediator.Publish(domainEvent, cancellationToken);
        }
        
        // 2. Guardar eventos de integración en Outbox
        var integrationEvents = entitiesWithEvents.SelectMany(e => e.IntegrationEvents).ToList();
        
        foreach (var integrationEvent in integrationEvents)
        {
            var outboxMessage = new OutboxMessage
            {
                Id = integrationEvent.Id,
                EventType = integrationEvent.GetType().AssemblyQualifiedName!,
                Content = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType()),
                OccurredOn = integrationEvent.OccurredOn
            };
            
            await outboxRepository.AddAsync(outboxMessage, cancellationToken);
            logger.LogDebug("Added integration event to outbox: {EventType}", integrationEvent.GetType().Name);
        }
        
        // 3. Limpiar eventos de las entidades
        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
            entity.ClearIntegrationEvents();
        }
        
        // 4. Guardar cambios (incluyendo mensajes de outbox)
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return response;
    }
}