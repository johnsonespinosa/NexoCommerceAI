using NexoCommerceAI.Domain.Common;

namespace NexoCommerceAI.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Order Order { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public string ProductSku { get; private set; }
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal TotalPrice => UnitPrice * Quantity;
    public string? ProductImageUrl { get; private set; }
    
    // Snapshot del producto en el momento de la orden
    public string? ProductSnapshot { get; private set; }
    
    private OrderItem() { }
    
    private OrderItem(Guid orderId, Guid productId, string productName, string productSku, 
                      decimal unitPrice, int quantity, string? productImageUrl, string? productSnapshot = null)
    {
        OrderId = orderId;
        ProductId = productId;
        ProductName = productName;
        ProductSku = productSku;
        UnitPrice = unitPrice;
        Quantity = quantity;
        ProductImageUrl = productImageUrl;
        ProductSnapshot = productSnapshot;
    }
    
    public static OrderItem Create(Guid orderId, Guid productId, string productName, string productSku,
                                    decimal unitPrice, int quantity, string? productImageUrl = null, string? productSnapshot = null)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("OrderId cannot be empty", nameof(orderId));
        
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId cannot be empty", nameof(productId));
        
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("ProductName cannot be empty", nameof(productName));
        
        if (string.IsNullOrWhiteSpace(productSku))
            throw new ArgumentException("ProductSku cannot be empty", nameof(productSku));
        
        if (unitPrice <= 0)
            throw new ArgumentException("UnitPrice must be greater than zero", nameof(unitPrice));
        
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        
        return new OrderItem(orderId, productId, productName, productSku, unitPrice, quantity, productImageUrl, productSnapshot);
    }
    
    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(newQuantity));
        
        Quantity = newQuantity;
        UpdatedAt = DateTime.UtcNow;
    }
}