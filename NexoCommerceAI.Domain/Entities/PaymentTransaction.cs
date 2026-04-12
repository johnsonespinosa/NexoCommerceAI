using NexoCommerceAI.Domain.Common;

namespace NexoCommerceAI.Domain.Entities;

public class PaymentTransaction : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Order Order { get; private set; }
    public string PaymentMethod { get; private set; }
    public string TransactionId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string? ProviderResponse { get; private set; }
    public string? Last4 { get; private set; }
    public string? CardType { get; private set; }
    public string? CardHolderName { get; private set; }
    public DateTime? RequestedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    
    private PaymentTransaction() { }
    
    private PaymentTransaction(Guid orderId, string paymentMethod, decimal amount)
    {
        OrderId = orderId;
        PaymentMethod = paymentMethod;
        Amount = amount;
        Status = PaymentStatus.Pending;
        RequestedAt = DateTime.UtcNow;
        TransactionId = GenerateTransactionId();
    }
    
    public static PaymentTransaction Create(Guid orderId, string paymentMethod, decimal amount)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("OrderId cannot be empty", nameof(orderId));
        
        if (string.IsNullOrWhiteSpace(paymentMethod))
            throw new ArgumentException("PaymentMethod cannot be empty", nameof(paymentMethod));
        
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero", nameof(amount));
        
        return new PaymentTransaction(orderId, paymentMethod, amount);
    }
    
    public void MarkAsCompleted(string transactionId, string? providerResponse = null, 
                                 string? last4 = null, string? cardType = null, string? cardHolderName = null)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
            throw new ArgumentException("TransactionId cannot be empty", nameof(transactionId));
        
        TransactionId = transactionId;
        Status = PaymentStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        ProviderResponse = providerResponse;
        Last4 = last4;
        CardType = cardType;
        CardHolderName = cardHolderName;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void MarkAsFailed(string errorMessage)
    {
        Status = PaymentStatus.Failed;
        ProviderResponse = errorMessage;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void MarkAsRefunded()
    {
        Status = PaymentStatus.Refunded;
        UpdatedAt = DateTime.UtcNow;
    }
    
    private static string GenerateTransactionId()
    {
        return $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    }
}

public enum PaymentStatus
{
    Pending = 1,
    Completed = 2,
    Failed = 3,
    Refunded = 4
}