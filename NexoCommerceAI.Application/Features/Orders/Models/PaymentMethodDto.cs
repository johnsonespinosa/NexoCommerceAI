namespace NexoCommerceAI.Application.Features.Orders.Models;

public record PaymentMethodDto
{
    public string Id { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string? Icon { get; init; }
}