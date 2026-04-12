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
    ILogger<DomainEventDispatcherBehavior<TRequest, TResponse>> logger,
    ICurrentUserService currentUserService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();
        
        // Obtener entidades con eventos de dominio
        var entitiesWithEvents = dbContext.ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Any() || e.Entity.IntegrationEvents.Any())
            .Select(e => e.Entity)
            .ToList();
        
        if (!entitiesWithEvents.Any())
            return response;
        
        var correlationId = Guid.NewGuid().ToString();
        var userId = currentUserService.UserId?.ToString() ?? "system";
        
        // 1. Publicar eventos de dominio (dentro de la transacción actual)
        var domainEvents = entitiesWithEvents.SelectMany(e => e.DomainEvents).ToList();
        
        foreach (var domainEvent in domainEvents)
        {
            logger.LogDebug("Publishing domain event: {EventType} for correlation {CorrelationId}", 
                domainEvent.GetType().Name, correlationId);
            await mediator.Publish(domainEvent, cancellationToken);
        }
        
        // 2. Guardar eventos de integración en Outbox
        var integrationEvents = entitiesWithEvents.SelectMany(e => e.IntegrationEvents).ToList();
        
        foreach (var integrationEvent in integrationEvents)
        {
            var aggregateId = GetAggregateId(integrationEvent);
            var outboxMessage = new OutboxMessage
            {
                Id = integrationEvent.Id,
                EventType = integrationEvent.GetType().AssemblyQualifiedName!,
                Content = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType()),
                OccurredOn = integrationEvent.OccurredOn,
                AggregateId = aggregateId,
                UserId = userId,
                CorrelationId = correlationId
            };
            
            await outboxRepository.AddAsync(outboxMessage, cancellationToken);
            logger.LogDebug("Added integration event to outbox: {EventType} for correlation {CorrelationId}", 
                integrationEvent.GetType().Name, correlationId);
        }
        
        // 3. Limpiar eventos de las entidades
        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
            entity.ClearIntegrationEvents();
        }
        
        // 4. Guardar cambios (incluyendo mensajes de outbox)
        await dbContext.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Dispatched {DomainCount} domain events and {IntegrationCount} integration events for correlation {CorrelationId}", 
            domainEvents.Count, integrationEvents.Count, correlationId);
        
        return response;
    }
    
    private static string? GetAggregateId(IIntegrationEvent integrationEvent)
    {
        // Intentar obtener el aggregate ID por reflexión
        var property = integrationEvent.GetType().GetProperty("OrderId") ??
                       integrationEvent.GetType().GetProperty("ProductId") ??
                       integrationEvent.GetType().GetProperty("UserId");
        
        return property?.GetValue(integrationEvent)?.ToString();
    }
}