using Microsoft.EntityFrameworkCore;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Infrastructure.Data.Repositories;

public class OutboxRepository(ApplicationDbContext dbContext) : IOutboxRepository
{
    public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<OutboxMessage>().AddAsync(message, cancellationToken);
    }
    
    public async Task<IReadOnlyList<OutboxMessage>> GetUnprocessedMessagesAsync(int take = 100, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<OutboxMessage>()
            .Where(m => !m.IsProcessed)
            .OrderBy(m => m.OccurredOn)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
    
    public async Task MarkAsProcessedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var message = await dbContext.Set<OutboxMessage>().FindAsync([id], cancellationToken);
        if (message != null)
        {
            message.ProcessedOn = DateTime.UtcNow;
        }
    }
    
    public async Task MarkAsFailedAsync(Guid id, string error, CancellationToken cancellationToken = default)
    {
        var message = await dbContext.Set<OutboxMessage>().FindAsync([id], cancellationToken);
        if (message != null)
        {
            message.Error = error;
            message.RetryCount++;
        }
    }
}