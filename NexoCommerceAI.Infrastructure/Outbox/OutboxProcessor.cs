// Infrastructure/Outbox/OutboxProcessor.cs

using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Common.Settings;
using NexoCommerceAI.Domain.Common;
using NexoCommerceAI.Domain.Entities;
using NexoCommerceAI.Infrastructure.Data;

namespace NexoCommerceAI.Infrastructure.Outbox;

public class OutboxProcessor(
    IServiceProvider serviceProvider,
    ILogger<OutboxProcessor> logger,
    IOptions<OutboxSettings> settings)
{
    private readonly OutboxSettings _settings = settings.Value;

    public async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();
        
        var messages = await outboxRepository.GetUnprocessedMessagesAsync(_settings.BatchSize, cancellationToken);
        
        logger.LogDebug("Processing {Count} outbox messages", messages.Count);
        
        foreach (var message in messages)
        {
            try
            {
                var eventType = Type.GetType(message.EventType);
                if (eventType == null)
                {
                    logger.LogWarning("Unknown event type: {EventType}", message.EventType);
                    await MarkAsFailedAsync(outboxRepository, dbContext, message, "Unknown event type", cancellationToken);
                    continue;
                }
                
                var domainEvent = JsonSerializer.Deserialize(message.Content, eventType) as IIntegrationEvent;
                if (domainEvent == null)
                {
                    logger.LogWarning("Failed to deserialize event: {EventType}", message.EventType);
                    await MarkAsFailedAsync(outboxRepository, dbContext, message, "Deserialization failed", cancellationToken);
                    continue;
                }
                
                await eventBus.PublishAsync(domainEvent, cancellationToken);
                await MarkAsProcessedAsync(outboxRepository, dbContext, message, cancellationToken);
                
                logger.LogDebug("Outbox message processed: {EventType} - {MessageId}", 
                    message.EventType, message.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process outbox message: {MessageId}", message.Id);
                await MarkAsFailedAsync(outboxRepository, dbContext, message, ex.Message, cancellationToken);
                
                // Si superó los reintentos, mover a dead letter
                if (message.RetryCount >= _settings.MaxRetryCount)
                {
                    await MoveToDeadLetterAsync(outboxRepository, dbContext, message, ex.Message, cancellationToken);
                }
            }
        }
        
        if (messages.Any())
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
    
    private static async Task MarkAsProcessedAsync(
        IOutboxRepository repository, 
        ApplicationDbContext dbContext,
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        await repository.MarkAsProcessedAsync(message.Id, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
    
    private static async Task MarkAsFailedAsync(
        IOutboxRepository repository,
        ApplicationDbContext dbContext,
        OutboxMessage message,
        string error,
        CancellationToken cancellationToken)
    {
        await repository.MarkAsFailedAsync(message.Id, error, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
    
    private async Task MoveToDeadLetterAsync(
        IOutboxRepository repository,
        ApplicationDbContext dbContext,
        OutboxMessage message,
        string error,
        CancellationToken cancellationToken)
    {
        logger.LogWarning("Moving message {MessageId} to dead letter after {RetryCount} retries", 
            message.Id, message.RetryCount);
        
        var deadLetter = new OutboxDeadLetter
        {
            Id = Guid.NewGuid(),
            OriginalMessageId = message.Id,
            EventType = message.EventType,
            Content = message.Content,
            Error = error,
            OccurredOn = message.OccurredOn,
            MovedToDeadLetterAt = DateTime.UtcNow
        };
        
        await dbContext.Set<OutboxDeadLetter>().AddAsync(deadLetter, cancellationToken);
        await repository.DeleteAsync(message.Id, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}