using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Wishlists.Models;
using NexoCommerceAI.Application.Features.Wishlists.Queries;

namespace NexoCommerceAI.Application.Features.Wishlists.Handlers;

public class GetWishlistQueryHandler(
    IWishlistRepository wishlistRepository,
    ILogger<GetWishlistQueryHandler> logger)
    : IRequestHandler<GetWishlistQuery, WishlistResponse?>
{
    public async Task<WishlistResponse?> Handle(GetWishlistQuery request, CancellationToken cancellationToken)
    {
        logger.LogDebug("Getting wishlist for user {UserId}", request.UserId);
        
        var wishlist = await wishlistRepository.GetDefaultByUserIdAsync(request.UserId, cancellationToken);
        
        if (wishlist == null)
            return null;
        
        return new WishlistResponse
        {
            Id = wishlist.Id,
            Name = wishlist.Name,
            IsDefault = wishlist.IsDefault,
            Items = wishlist.Items.Select(i => new WishlistItemResponse
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                ProductImageUrl = i.ProductImageUrl,
                Price = i.Price,
                AddedAt = i.CreatedAt
            }).ToList(),
            TotalItems = wishlist.TotalItems
        };
    }
}