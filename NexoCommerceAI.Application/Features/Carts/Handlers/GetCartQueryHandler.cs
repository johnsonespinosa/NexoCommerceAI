using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Carts.Models;
using NexoCommerceAI.Application.Features.Carts.Queries;

namespace NexoCommerceAI.Application.Features.Carts.Handlers;

public class GetCartQueryHandler(
    ICartRepository cartRepository,
    ICartCacheService cartCacheService,
    ILogger<GetCartQueryHandler> logger)
    : IRequestHandler<GetCartQuery, CartResponse?>
{
    public async Task<CartResponse?> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        logger.LogDebug("Getting cart for user {UserId}", request.UserId);
        
        // Intentar obtener del caché primero
        var cachedCart = await cartCacheService.GetCartAsync(request.UserId, cancellationToken);
        if (cachedCart != null)
        {
            logger.LogDebug("Cart retrieved from cache for user {UserId}", request.UserId);
            return cachedCart;
        }
        
        // Si no está en caché, obtener de la base de datos
        var cart = await cartRepository.GetByUserIdWithItemsAsync(request.UserId, cancellationToken);
        
        if (cart == null)
            return null;
        
        var response = CartResponse.MapToCartResponse(cart);
        
        // Guardar en caché para futuras consultas
        await cartCacheService.SetCartAsync(request.UserId, response, cancellationToken: cancellationToken);
        
        return response;
    }
}