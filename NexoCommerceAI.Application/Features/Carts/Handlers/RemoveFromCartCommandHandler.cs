using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Carts.Commands;
using NexoCommerceAI.Application.Features.Carts.Models;

namespace NexoCommerceAI.Application.Features.Carts.Handlers;

public class RemoveFromCartCommandHandler(
    ICartRepository cartRepository,
    ICartCacheService cartCacheService,
    ILogger<RemoveFromCartCommandHandler> logger)
    : IRequestHandler<RemoveFromCartCommand, CartResponse>
{
    public async Task<CartResponse> Handle(RemoveFromCartCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Removing product {ProductId} from cart for user {UserId}", 
            request.ProductId, request.UserId);

        var cart = await cartRepository.GetByUserIdWithItemsAsync(request.UserId, cancellationToken);
        if (cart == null)
            throw new NotFoundException($"Cart not found for user {request.UserId}");
        
        cart.RemoveItem(request.ProductId);
        
        await cartRepository.UpdateAsync(cart, cancellationToken);
        await cartRepository.SaveChangesAsync(cancellationToken);
        
        var response = CartResponse.MapToCartResponse(cart);
        
        await cartCacheService.SetCartAsync(request.UserId, response, cancellationToken: cancellationToken);
        
        return response;
    }
}