namespace NexoCommerceAI.Application.Features.Orders.Models;

public record PaymentResultResponse
{
    public bool Success { get; init; }
    public string TransactionId { get; init; } = default!;
    public string? ErrorMessage { get; init; }
    public string? Last4 { get; init; }
    public string? CardType { get; init; }
}