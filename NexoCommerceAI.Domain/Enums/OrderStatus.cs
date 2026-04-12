namespace NexoCommerceAI.Domain.Enums;

public enum OrderStatus
{
    Pending = 1,
    PaymentProcessing = 2,
    Paid = 3,
    Shipped = 4,
    Delivered = 5,
    Cancelled = 6,
    Refunded = 7
}