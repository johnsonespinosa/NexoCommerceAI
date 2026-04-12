using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Carts.Models;

namespace NexoCommerceAI.Infrastructure.Services;

public class CartCacheService(ICacheService cache, ILogger<CartCacheService> logger) : ICartCacheService
{
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromHours(24);

    public async Task<CartResponse?> GetCartAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = GetCacheKey(userId);
            return await cache.GetAsync<CartResponse>(key, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting cart from cache for user {UserId}", userId);
            return null;
        }
    }
    
    public async Task SetCartAsync(Guid userId, CartResponse cart, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = GetCacheKey(userId);
            await cache.SetAsync(key, cart, expiration ?? _defaultExpiration, cancellationToken);
            logger.LogDebug("Cart cached for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error setting cart in cache for user {UserId}", userId);
        }
    }
    
    public async Task RemoveCartAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = GetCacheKey(userId);
            await cache.RemoveAsync(key, cancellationToken);
            logger.LogDebug("Cart removed from cache for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing cart from cache for user {UserId}", userId);
        }
    }
    
    public async Task<bool> CartExistsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = GetCacheKey(userId);
            var cart = await cache.GetAsync<CartResponse>(key, cancellationToken);
            return cart != null;
        }
        catch
        {
            return false;
        }
    }
    
    private static string GetCacheKey(Guid userId) => $"cart:{userId}";
}