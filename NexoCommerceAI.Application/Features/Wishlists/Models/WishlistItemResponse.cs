namespace NexoCommerceAI.Application.Features.Wishlists.Models;

public record WishlistItemResponse
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = default!;
    public string? ProductImageUrl { get; init; }
    public decimal Price { get; init; }
    public decimal? CompareAtPrice { get; init; }
    public bool IsOnSale { get; init; }
    public string? Notes { get; init; }
    public DateTime AddedAt { get; init; }
}