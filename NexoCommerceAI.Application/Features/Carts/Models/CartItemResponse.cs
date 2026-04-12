namespace NexoCommerceAI.Application.Features.Carts.Models;

public record CartItemResponse
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = default!;
    public string? ProductImageUrl { get; init; }
    public decimal UnitPrice { get; init; }
    public int Quantity { get; init; }
    public decimal TotalPrice { get; init; }
    public bool IsInStock { get; init; }
}