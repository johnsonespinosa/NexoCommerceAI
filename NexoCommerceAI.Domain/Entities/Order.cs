using NexoCommerceAI.Domain.Common;
using NexoCommerceAI.Domain.Enums;
using NexoCommerceAI.Domain.Events;
using NexoCommerceAI.Domain.Extensions;
using NexoCommerceAI.Domain.ValueObjects;

namespace NexoCommerceAI.Domain.Entities;

public class Order : BaseEntity
{
    private readonly List<OrderItem> _items = [];
    private readonly List<OrderStatusHistory> _statusHistory = [];
    
    public string OrderNumber { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; }
    public OrderStatus Status { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public IReadOnlyCollection<OrderStatusHistory> StatusHistory => _statusHistory.AsReadOnly();
    
    // Totales
    public Money Subtotal { get; private set; }
    public Money TaxAmount { get; private set; }
    public Money ShippingAmount { get; private set; }
    public Money DiscountAmount { get; private set; }
    public Money TotalAmount { get; private set; }
    
    // Direcciones
    public Address ShippingAddress { get; private set; }
    public Address BillingAddress { get; private set; }
    
    // Información de envío
    public string? TrackingNumber { get; private set; }
    public string? CarrierName { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    
    // Notas
    public string? CustomerNotes { get; private set; }
    public string? AdminNotes { get; private set; }
    
    private Order() { }
    
    private Order(Guid userId, Address shippingAddress, Address billingAddress, string? customerNotes = null)
    {
        OrderNumber = GenerateOrderNumber();
        UserId = userId;
        ShippingAddress = shippingAddress;
        BillingAddress = billingAddress;
        CustomerNotes = customerNotes;
        Status = OrderStatus.Pending;
        
        Subtotal = Money.Zero();
        TaxAmount = Money.Zero();
        ShippingAmount = Money.Zero();
        DiscountAmount = Money.Zero();
        TotalAmount = Money.Zero();
        
        AddDomainEvent(new OrderCreatedEvent(Id, OrderNumber, userId));
    }
    
    public static Order Create(Guid userId, Address shippingAddress, Address billingAddress, string? customerNotes = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty", nameof(userId));
        
        return new Order(userId, shippingAddress, billingAddress, customerNotes);
    }
    
    public void AddItem(Product product, int quantity, decimal unitPrice)
    {
        var item = OrderItem.Create(Id, product.Id, product.Name, product.Sku, unitPrice, quantity, product.ImageUrl);
        _items.Add(item);
        
        RecalculateTotals();
    }
    
    public void UpdateStatus(OrderStatus newStatus, string? comment = null, string? changedBy = null)
    {
        if (!CanTransitionTo(newStatus))
            throw new InvalidOperationException($"Cannot transition from {Status} to {newStatus}");
        
        var oldStatus = Status;
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
        
        var history = OrderStatusHistory.Create(Id, oldStatus, newStatus, changedBy, comment);
        _statusHistory.Add(history);
        
        switch (newStatus)
        {
            case OrderStatus.Shipped:
                ShippedAt = DateTime.UtcNow;
                AddDomainEvent(new OrderShippedEvent(Id, OrderNumber, TrackingNumber));
                break;
            case OrderStatus.Delivered:
                DeliveredAt = DateTime.UtcNow;
                AddDomainEvent(new OrderDeliveredEvent(Id, OrderNumber));
                break;
            case OrderStatus.Cancelled:
                CancelledAt = DateTime.UtcNow;
                AddDomainEvent(new OrderCancelledEvent(Id, OrderNumber));
                break;
        }
        
        AddDomainEvent(new OrderStatusChangedEvent(Id, OrderNumber, oldStatus, newStatus));
    }
    
    public void UpdateShippingInfo(string trackingNumber, string carrierName)
    {
        if (string.IsNullOrWhiteSpace(trackingNumber))
            throw new ArgumentException("Tracking number is required", nameof(trackingNumber));
        
        if (string.IsNullOrWhiteSpace(carrierName))
            throw new ArgumentException("Carrier name is required", nameof(carrierName));
        
        TrackingNumber = trackingNumber;
        CarrierName = carrierName;
        UpdatedAt = DateTime.UtcNow;
        
        if (Status == OrderStatus.Paid)
            UpdateStatus(OrderStatus.Shipped);
    }
    
    public void Cancel(string? reason = null)
    {
        if (Status == OrderStatus.Delivered)
            throw new InvalidOperationException("Cannot cancel delivered order");
        
        if (Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Order is already cancelled");
        
        UpdateStatus(OrderStatus.Cancelled, reason);
    }
    
    public void AddAdminNotes(string notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
            throw new ArgumentException("Notes cannot be empty", nameof(notes));
        
        AdminNotes = notes;
        UpdatedAt = DateTime.UtcNow;
    }
    
    private void RecalculateTotals()
    {
        var subtotal = _items.Sum(i => i.TotalPrice);
        Subtotal = new Money(subtotal);
        
        // 21% IVA (ejemplo)
        TaxAmount = new Money(subtotal * 0.21m);
        
        // Calcular total
        var total = subtotal + TaxAmount.Amount + ShippingAmount.Amount - DiscountAmount.Amount;
        TotalAmount = new Money(total);
    }
    
    private bool CanTransitionTo(OrderStatus newStatus)
    {
        return Status.GetAllowedTransitions().Contains(newStatus);
    }
    
    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    }
}

