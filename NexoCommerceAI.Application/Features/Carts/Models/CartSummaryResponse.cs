namespace NexoCommerceAI.Application.Features.Carts.Models;

/// <summary>
/// Cart summary response.
/// </summary>
public record CartSummaryResponse
{
    public int TotalItems { get; init; }
    public decimal TotalAmount { get; init; }
    public int ItemCount { get; init; }
}