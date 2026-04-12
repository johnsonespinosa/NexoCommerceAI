using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Wishlists.Commands;
using NexoCommerceAI.Application.Features.Wishlists.Models;

namespace NexoCommerceAI.Application.Features.Wishlists.Handlers;

public class RemoveFromWishlistCommandHandler(
    IWishlistRepository wishlistRepository,
    ILogger<RemoveFromWishlistCommandHandler> logger)
    : IRequestHandler<RemoveFromWishlistCommand, WishlistResponse>
{
    public async Task<WishlistResponse> Handle(RemoveFromWishlistCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Removing product {ProductId} from wishlist for user {UserId}", 
            request.ProductId, request.UserId);

        var wishlist = await wishlistRepository.GetDefaultByUserIdAsync(request.UserId, cancellationToken);
        if (wishlist == null)
            throw new NotFoundException($"Wishlist not found for user {request.UserId}");
        
        wishlist.RemoveItem(request.ProductId);
        
        await wishlistRepository.UpdateAsync(wishlist, cancellationToken);
        await wishlistRepository.SaveChangesAsync(cancellationToken);
        
        return WishlistResponse.MapToWishlistResponse(wishlist);
    }
}