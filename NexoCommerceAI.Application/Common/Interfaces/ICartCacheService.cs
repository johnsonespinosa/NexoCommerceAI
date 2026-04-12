using NexoCommerceAI.Application.Features.Carts.Models;

namespace NexoCommerceAI.Application.Common.Interfaces;

public interface ICartCacheService
{
    Task<CartResponse?> GetCartAsync(Guid userId, CancellationToken cancellationToken = default);
    Task SetCartAsync(Guid userId, CartResponse cart, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveCartAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> CartExistsAsync(Guid userId, CancellationToken cancellationToken = default);
}