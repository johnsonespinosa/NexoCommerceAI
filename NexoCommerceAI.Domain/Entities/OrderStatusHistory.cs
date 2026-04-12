using NexoCommerceAI.Domain.Common;
using NexoCommerceAI.Domain.Enums;

namespace NexoCommerceAI.Domain.Entities;

public class OrderStatusHistory : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Order Order { get; private set; }
    public OrderStatus OldStatus { get; private set; }
    public OrderStatus NewStatus { get; private set; }
    public string? ChangedBy { get; private set; }
    public string? Comment { get; private set; }
    public DateTime ChangedAt { get; private set; }
    
    private OrderStatusHistory() { }
    
    private OrderStatusHistory(Guid orderId, OrderStatus oldStatus, OrderStatus newStatus, string? changedBy, string? comment)
    {
        OrderId = orderId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        ChangedBy = changedBy;
        Comment = comment;
        ChangedAt = DateTime.UtcNow;
    }
    
    public static OrderStatusHistory Create(Guid orderId, OrderStatus oldStatus, OrderStatus newStatus, string? changedBy = null, string? comment = null)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("OrderId cannot be empty", nameof(orderId));
        
        return new OrderStatusHistory(orderId, oldStatus, newStatus, changedBy, comment);
    }
}