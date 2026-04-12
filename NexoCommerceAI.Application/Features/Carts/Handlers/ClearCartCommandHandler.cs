using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Carts.Commands;

namespace NexoCommerceAI.Application.Features.Carts.Handlers;

public class ClearCartCommandHandler(
    ICartRepository cartRepository,
    ICartCacheService cartCacheService,
    ILogger<ClearCartCommandHandler> logger)
    : IRequestHandler<ClearCartCommand, bool>
{
    public async Task<bool> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Clearing cart for user {UserId}", request.UserId);

        var cart = await cartRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (cart == null)
            return false;
        
        cart.Clear();
        
        await cartRepository.UpdateAsync(cart, cancellationToken);
        await cartRepository.SaveChangesAsync(cancellationToken);
        
        await cartCacheService.RemoveCartAsync(request.UserId, cancellationToken);
        
        logger.LogInformation("Cart cleared for user {UserId}", request.UserId);
        
        return true;
    }
}