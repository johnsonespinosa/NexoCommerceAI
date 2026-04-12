using Microsoft.EntityFrameworkCore;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Infrastructure.Data.Repositories;

public class CartRepository(ApplicationDbContext dbContext) 
    : RepositoryAsync<Cart>(dbContext), ICartRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Carts
            .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted, cancellationToken);
    }
    
    public async Task<Cart?> GetByUserIdWithItemsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted, cancellationToken);
    }
    
    public async Task<IReadOnlyList<Cart>> GetAbandonedCartsAsync(TimeSpan abandonedSince, CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow.Subtract(abandonedSince);
        
        return await _dbContext.Carts
            .Include(c => c.Items)
            .Where(c => !c.IsDeleted && 
                        c.LastUpdatedAt < cutoff && 
                        !c.IsAbandoned && 
                        c.Items.Any())
            .ToListAsync(cancellationToken);
    }
}