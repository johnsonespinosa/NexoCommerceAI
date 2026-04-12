using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Wishlists.Models;

public record WishlistResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = default!;
    public bool IsDefault { get; init; }
    public IReadOnlyList<WishlistItemResponse> Items { get; init; } = new List<WishlistItemResponse>();
    public int TotalItems { get; init; }
    
    public static WishlistResponse MapToWishlistResponse(Wishlist wishlist)
    {
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
                CompareAtPrice = null, // Se podría obtener del producto
                IsOnSale = false,
                Notes = i.Notes,
                AddedAt = i.CreatedAt
            }).ToList(),
            TotalItems = wishlist.TotalItems
        };
    }
}