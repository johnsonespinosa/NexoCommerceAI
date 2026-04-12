using Microsoft.EntityFrameworkCore;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Infrastructure.Data.Repositories;

public class WebhookEventRepository(ApplicationDbContext dbContext) : IWebhookEventRepository
{
    public async Task<bool> HasBeenProcessedAsync(string eventId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<ProcessedWebhookEvent>()
            .AnyAsync(e => e.EventId == eventId, cancellationToken);
    }
    
    public async Task MarkAsProcessedAsync(string eventId, string eventType, string? orderId = null, CancellationToken cancellationToken = default)
    {
        var processedEvent = new ProcessedWebhookEvent
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            EventType = eventType,
            OrderId = orderId,
            ProcessedAt = DateTime.UtcNow
        };
        
        await dbContext.Set<ProcessedWebhookEvent>().AddAsync(processedEvent, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}