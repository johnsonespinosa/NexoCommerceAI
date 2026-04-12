using NexoCommerceAI.Domain.Common;

namespace NexoCommerceAI.Domain.Entities;

public class PaymentHistory : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Order Order { get; private set; } = default!;
    public string EventType { get; private set; } = default!;
    public string? PaymentIntentId { get; private set; }
    public decimal Amount { get; private set; }
    public string Status { get; private set; } = default!;
    public string? ErrorMessage { get; private set; }
    public string? Metadata { get; private set; }
    public DateTime OccurredAt { get; private set; }
    
    private PaymentHistory() { }
    
    private PaymentHistory(Guid orderId, string eventType, decimal amount, string status, string? paymentIntentId = null, string? errorMessage = null, string? metadata = null)
    {
        OrderId = orderId;
        EventType = eventType;
        PaymentIntentId = paymentIntentId;
        Amount = amount;
        Status = status;
        ErrorMessage = errorMessage;
        Metadata = metadata;
        OccurredAt = DateTime.UtcNow;
    }
    
    public static PaymentHistory CreatePaymentIntentCreated(Guid orderId, string paymentIntentId, decimal amount)
    {
        return new PaymentHistory(orderId, "payment_intent.created", amount, "processing", paymentIntentId);
    }
    
    public static PaymentHistory CreatePaymentSucceeded(Guid orderId, string paymentIntentId, decimal amount, string metadata)
    {
        return new PaymentHistory(orderId, "payment_intent.succeeded", amount, "completed", paymentIntentId, metadata: metadata);
    }
    
    public static PaymentHistory CreatePaymentFailed(Guid orderId, string paymentIntentId, decimal amount, string errorMessage)
    {
        return new PaymentHistory(orderId, "payment_intent.failed", amount, "failed", paymentIntentId, errorMessage);
    }
    
    public static PaymentHistory CreateRefunded(Guid orderId, string paymentIntentId, decimal amount)
    {
        return new PaymentHistory(orderId, "charge.refunded", amount, "refunded", paymentIntentId);
    }
}