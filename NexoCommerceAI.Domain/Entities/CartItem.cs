using NexoCommerceAI.Domain.Common;

namespace NexoCommerceAI.Domain.Entities;

public class CartItem : BaseEntity
{
    public Guid CartId { get; private set; }
    public Cart Cart { get; private set; } = default!;
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = default!;
    public string ProductName { get; private set; } = default!;
    public decimal UnitPrice { get; private set; }
    public string? ProductImageUrl { get; private set; }
    public int Quantity { get; private set; }
    public decimal TotalPrice => UnitPrice * Quantity;
    
    private CartItem() { }
    
    private CartItem(Guid cartId, Guid productId, string productName, decimal unitPrice, string? productImageUrl, int quantity)
    {
        CartId = cartId;
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        ProductImageUrl = productImageUrl;
        Quantity = quantity;
    }
    
    public static CartItem Create(Guid cartId, Guid productId, string productName, decimal unitPrice, string? productImageUrl, int quantity)
    {
        if (cartId == Guid.Empty)
            throw new ArgumentException("CartId cannot be empty", nameof(cartId));
        
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId cannot be empty", nameof(productId));
        
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("ProductName cannot be empty", nameof(productName));
        
        if (unitPrice <= 0)
            throw new ArgumentException("UnitPrice must be greater than zero", nameof(unitPrice));
        
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        
        return new CartItem(cartId, productId, productName, unitPrice, productImageUrl, quantity);
    }
    
    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(newQuantity));
        
        Quantity = newQuantity;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void UpdateUnitPrice(decimal newPrice)
    {
        if (newPrice <= 0)
            throw new ArgumentException("Price must be greater than zero", nameof(newPrice));
        
        UnitPrice = newPrice;
        UpdatedAt = DateTime.UtcNow;
    }
}