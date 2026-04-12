namespace NexoCommerceAI.Application.Features.Carts.Models;

public record CartSummary
{
    public int TotalItems { get; init; }
    public decimal TotalAmount { get; init; }
}