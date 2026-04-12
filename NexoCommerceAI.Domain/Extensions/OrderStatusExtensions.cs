using NexoCommerceAI.Domain.Enums;

namespace NexoCommerceAI.Domain.Extensions;

public static class OrderStatusExtensions
{
    public static OrderStatus[] GetAllowedTransitions(this OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => [OrderStatus.PaymentProcessing, OrderStatus.Cancelled],
            OrderStatus.PaymentProcessing => [OrderStatus.Paid, OrderStatus.Cancelled],
            OrderStatus.Paid => [OrderStatus.Shipped, OrderStatus.Cancelled, OrderStatus.Refunded],
            OrderStatus.Shipped => [OrderStatus.Delivered],
            _ => Array.Empty<OrderStatus>()
        };
    }
    
    public static string GetDisplayName(this OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => "Pending",
            OrderStatus.PaymentProcessing => "Processing Payment",
            OrderStatus.Paid => "Paid",
            OrderStatus.Shipped => "Shipped",
            OrderStatus.Delivered => "Delivered",
            OrderStatus.Cancelled => "Cancelled",
            OrderStatus.Refunded => "Refunded",
            _ => status.ToString()
        };
    }
}