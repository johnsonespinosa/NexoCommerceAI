namespace NexoCommerceAI.Application.Features.Payments.Models;

public record PaymentInfoRequest
{
    public string PaymentMethodId { get; init; } = default!;
    public string CardNumber { get; init; } = default!;
    public string ExpiryMonth { get; init; } = default!;
    public string ExpiryYear { get; init; } = default!;
    public string Cvv { get; init; } = default!;
    public string CardHolderName { get; init; } = default!;
}