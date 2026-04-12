using NexoCommerceAI.Domain.Common;
using NexoCommerceAI.Domain.Events;

namespace NexoCommerceAI.Domain.Entities;

public class Cart : BaseEntity
{
    private readonly List<CartItem> _items = [];
    
    public Guid UserId { get; private set; }
    public User User { get; private set; } = default!;
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();
    public decimal TotalAmount => _items.Sum(i => i.TotalPrice);
    public int TotalItems => _items.Sum(i => i.Quantity);
    public DateTime? LastUpdatedAt { get; private set; }
    public bool IsAbandoned { get; private set; }
    public DateTime? AbandonedAt { get; private set; }

    private Cart(Guid userId)
    {
        UserId = userId;
        IsActive = true;
        LastUpdatedAt = DateTime.UtcNow;
    }
    
    public static Cart Create(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty", nameof(userId));
        
        return new Cart(userId);
    }
    
    public void AddItem(Product product, int quantity, decimal? selectedPrice = null)
    {
        ArgumentNullException.ThrowIfNull(product);
        
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        
        var existingItem = _items.FirstOrDefault(i => i.ProductId == product.Id);
        
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var productImageUrl = product.ImageUrl;
        
            var item = CartItem.Create(Id, product.Id, product.Name, selectedPrice ?? product.Price, 
                productImageUrl, quantity);
            _items.Add(item);
        }
        
        LastUpdatedAt = DateTime.UtcNow;
        IsAbandoned = false;
        AbandonedAt = null;
        
        AddDomainEvent(new CartItemAddedEvent(UserId, product.Id, product.Name, quantity));
    }
    
    public void UpdateItemQuantity(Guid productId, int quantity)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
            throw new InvalidOperationException($"Product {productId} not in cart");
        
        if (quantity <= 0)
        {
            RemoveItem(productId);
            return;
        }
        
        var oldQuantity = item.Quantity;
        item.UpdateQuantity(quantity);
        LastUpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new CartItemQuantityUpdatedEvent(UserId, productId, oldQuantity, quantity));
    }
    
    public void RemoveItem(Guid productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
            throw new InvalidOperationException($"Product {productId} not in cart");
        
        _items.Remove(item);
        LastUpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new CartItemRemovedEvent(UserId, productId));
    }
    
    public void Clear()
    {
        _items.Clear();
        LastUpdatedAt = DateTime.UtcNow;
    }
    
    public void MarkAsAbandoned()
    {
        IsAbandoned = true;
        AbandonedAt = DateTime.UtcNow;
        
        AddDomainEvent(new CartAbandonedEvent(UserId, TotalAmount, TotalItems, _items.ToList()));
    }
    
    public void MergeCart(Cart guestCart)
    {
        ArgumentNullException.ThrowIfNull(guestCart);
        
        foreach (var guestItem in guestCart.Items)
        {
            AddItem(guestItem.Product, guestItem.Quantity, guestItem.UnitPrice);
        }
        
        LastUpdatedAt = DateTime.UtcNow;
    }
}