using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Wishlists.Commands;
using NexoCommerceAI.Application.Features.Wishlists.Models;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Wishlists.Handlers;

public class AddToWishlistCommandHandler(
    IWishlistRepository wishlistRepository,
    IProductRepository productRepository,
    ILogger<AddToWishlistCommandHandler> logger)
    : IRequestHandler<AddToWishlistCommand, WishlistResponse>
{
    public async Task<WishlistResponse> Handle(AddToWishlistCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Adding product {ProductId} to wishlist for user {UserId}", 
            request.ProductId, request.UserId);

        // Verificar que el producto existe
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
            throw new NotFoundException(nameof(Product), request.ProductId);
        
        // Obtener la wishlist (por defecto o personalizada)
        Wishlist wishlist;
        
        if (!string.IsNullOrWhiteSpace(request.WishlistName))
        {
            // Buscar wishlist personalizada
            var customWishlist = await wishlistRepository.GetByUserIdWithItemsAsync(request.UserId, cancellationToken);
            if (customWishlist != null && customWishlist.Name == request.WishlistName)
            {
                wishlist = customWishlist;
            }
            else
            {
                wishlist = Wishlist.CreateCustom(request.UserId, request.WishlistName);
                await wishlistRepository.AddAsync(wishlist, cancellationToken);
            }
        }
        else
        {
            // Obtener o crear wishlist por defecto
            wishlist = await wishlistRepository.GetDefaultByUserIdAsync(request.UserId, cancellationToken);
            if (wishlist == null)
            {
                wishlist = Wishlist.CreateDefault(request.UserId);
                await wishlistRepository.AddAsync(wishlist, cancellationToken);
            }
        }
        
        wishlist.AddItem(product);
        
        await wishlistRepository.UpdateAsync(wishlist, cancellationToken);
        await wishlistRepository.SaveChangesAsync(cancellationToken);
        
        var response = WishlistResponse.MapToWishlistResponse(wishlist);
        
        logger.LogInformation("Product {ProductId} added to wishlist for user {UserId}", 
            request.ProductId, request.UserId);
        
        return response;
    }
    
    
}