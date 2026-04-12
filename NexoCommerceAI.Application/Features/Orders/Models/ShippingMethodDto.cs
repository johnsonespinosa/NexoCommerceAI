namespace NexoCommerceAI.Application.Features.Orders.Models;

public record ShippingMethodDto
{
    public string Id { get; init; } = default!;
    public string Name { get; init; } = default!;
    public decimal Price { get; init; }
    public int EstimatedDays { get; init; }
}