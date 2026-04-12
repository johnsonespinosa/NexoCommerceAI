namespace NexoCommerceAI.Application.Features.Payments.Models;

public record PaymentMethodDto
{
    public string Id { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string? Icon { get; init; }
}