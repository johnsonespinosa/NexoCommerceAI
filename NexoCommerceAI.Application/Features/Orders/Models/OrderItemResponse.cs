namespace NexoCommerceAI.Application.Features.Orders.Models;

public record OrderItemResponse
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = default!;
    public string ProductSku { get; init; } = default!;
    public decimal UnitPrice { get; init; }
    public int Quantity { get; init; }
    public decimal TotalPrice { get; init; }
    public string? ProductImageUrl { get; init; }
}