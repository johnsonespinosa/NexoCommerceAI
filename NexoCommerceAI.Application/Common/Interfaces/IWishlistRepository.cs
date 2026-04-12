using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Common.Interfaces;

public interface IWishlistRepository : IRepositoryAsync<Wishlist>
{
    Task<Wishlist?> GetDefaultByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Wishlist?> GetByUserIdWithItemsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsInWishlistAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);
}