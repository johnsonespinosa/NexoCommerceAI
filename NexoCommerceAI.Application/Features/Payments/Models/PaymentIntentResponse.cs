namespace NexoCommerceAI.Application.Features.Payments.Models;

public record PaymentIntentResponse
{
    public string PaymentIntentId { get; init; } = default!;
    public string ClientSecret { get; init; } = default!;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = default!;
    public string Status { get; init; } = default!;
}