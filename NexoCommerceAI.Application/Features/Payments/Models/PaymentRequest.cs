namespace NexoCommerceAI.Application.Features.Payments.Models;

public class PaymentRequest
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string? PaymentMethodId { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CardNumber { get; set; }
    public string? ExpiryMonth { get; set; }
    public string? ExpiryYear { get; set; }
    public string? Cvv { get; set; }
    public string? CardHolderName { get; set; }
}