using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Carts.Commands;
using NexoCommerceAI.Application.Features.Carts.Models;

namespace NexoCommerceAI.Application.Features.Carts.Handlers;

public class UpdateCartItemQuantityCommandHandler(
    ICartRepository cartRepository,
    IProductRepository productRepository,
    ICartCacheService cartCacheService,
    ILogger<UpdateCartItemQuantityCommandHandler> logger)
    : IRequestHandler<UpdateCartItemQuantityCommand, CartResponse>
{
    public async Task<CartResponse> Handle(UpdateCartItemQuantityCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating quantity for product {ProductId} in cart for user {UserId}, New Quantity: {Quantity}", 
            request.ProductId, request.UserId, request.Quantity);

        var cart = await cartRepository.GetByUserIdWithItemsAsync(request.UserId, cancellationToken);
        if (cart == null)
            throw new NotFoundException($"Cart not found for user {request.UserId}");
        
        if (request.Quantity > 0)
        {
            // Verificar stock disponible
            var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product != null && product.Stock < request.Quantity)
                throw new ValidationException($"Insufficient stock for product '{product.Name}'. Available: {product.Stock}");
        }
        
        cart.UpdateItemQuantity(request.ProductId, request.Quantity);
        
        await cartRepository.UpdateAsync(cart, cancellationToken);
        await cartRepository.SaveChangesAsync(cancellationToken);
        
        var response = CartResponse.MapToCartResponse(cart);
        
        await cartCacheService.SetCartAsync(request.UserId, response, cancellationToken: cancellationToken);
        
        return response;
    }
    
    
}