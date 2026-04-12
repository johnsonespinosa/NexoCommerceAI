using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Carts.Models;

public record CartResponse
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public IReadOnlyList<CartItemResponse> Items { get; init; } = new List<CartItemResponse>();
    public decimal TotalAmount { get; init; }
    public int TotalItems { get; init; }
    public DateTime LastUpdatedAt { get; init; }
    
    public static CartResponse MapToCartResponse(Cart cart)
    {
        return new CartResponse
        {
            Id = cart.Id,
            UserId = cart.UserId,
            Items = cart.Items.Select(i => new CartItemResponse
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                ProductImageUrl = i.ProductImageUrl,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                TotalPrice = i.TotalPrice,
                IsInStock = true
            }).ToList(),
            TotalAmount = cart.TotalAmount,
            TotalItems = cart.TotalItems,
            LastUpdatedAt = cart.LastUpdatedAt ?? cart.CreatedAt
        };
    }
}

