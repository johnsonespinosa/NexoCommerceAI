namespace NexoCommerceAI.Application.Features.Payments.Models;

public record ConfirmPaymentResponse
{
    public bool Success { get; init; }
    public string OrderNumber { get; init; } = default!;
    public string Status { get; init; } = default!;
    public string? TransactionId { get; init; }
    public string? ErrorMessage { get; init; }
}