using Microsoft.EntityFrameworkCore;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Infrastructure.Data.Repositories;

public class WishlistRepository(ApplicationDbContext dbContext) 
    : RepositoryAsync<Wishlist>(dbContext), IWishlistRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<Wishlist?> GetDefaultByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Wishlists
            .Include(w => w.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(w => w.UserId == userId && w.IsDefault && !w.IsDeleted, cancellationToken);
    }
    
    public async Task<Wishlist?> GetByUserIdWithItemsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Wishlists
            .Include(w => w.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(w => w.UserId == userId && !w.IsDeleted, cancellationToken);
    }
    
    public async Task<bool> IsInWishlistAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.WishlistItems
            .AnyAsync(i => i.Wishlist.UserId == userId && i.ProductId == productId && !i.IsDeleted, cancellationToken);
    }
}