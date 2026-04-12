namespace NexoCommerceAI.Application.Features.Payments.Models;

public class PaymentResult
{
    public bool Success { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string? ClientSecret { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Last4 { get; set; }
    public string? CardType { get; set; }
}