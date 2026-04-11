using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Domain.Common;
using NexoCommerceAI.Infrastructure.Data;

namespace NexoCommerceAI.Infrastructure.Outbox;

public class OutboxProcessor(IServiceProvider serviceProvider, ILogger<OutboxProcessor> logger)
{
    public async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();
        
        var messages = await outboxRepository.GetUnprocessedMessagesAsync(50, cancellationToken);
        
        foreach (var message in messages)
        {
            try
            {
                var eventType = Type.GetType(message.EventType);
                if (eventType == null)
                {
                    logger.LogWarning("Unknown event type: {EventType}", message.EventType);
                    continue;
                }
                
                var domainEvent = JsonSerializer.Deserialize(message.Content, eventType) as IIntegrationEvent;
                if (domainEvent == null)
                {
                    logger.LogWarning("Failed to deserialize event: {EventType}", message.EventType);
                    continue;
                }
                
                await eventBus.PublishAsync(domainEvent, cancellationToken);
                await outboxRepository.MarkAsProcessedAsync(message.Id, cancellationToken);
                
                logger.LogInformation("Outbox message processed: {EventType} - {MessageId}", 
                    message.EventType, message.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process outbox message: {MessageId}", message.Id);
                await outboxRepository.MarkAsFailedAsync(message.Id, ex.Message, cancellationToken);
            }
        }
        
        if (messages.Any())
        {
            await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().SaveChangesAsync(cancellationToken);
        }
    }
}