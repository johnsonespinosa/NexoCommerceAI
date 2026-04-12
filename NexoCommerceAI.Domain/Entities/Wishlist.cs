using NexoCommerceAI.Domain.Common;
using NexoCommerceAI.Domain.Events;

namespace NexoCommerceAI.Domain.Entities;

public class Wishlist : BaseEntity
{
    private readonly List<WishlistItem> _items = [];
    
    public Guid UserId { get; private set; }
    public User User { get; private set; } = default!;
    public IReadOnlyCollection<WishlistItem> Items => _items.AsReadOnly();
    public string Name { get; private set; }
    public bool IsDefault { get; private set; }
    public int TotalItems => _items.Count;

    private Wishlist(Guid userId, string name, bool isDefault = false)
    {
        UserId = userId;
        Name = name;
        IsDefault = isDefault;
        IsActive = true;
    }
    
    public static Wishlist CreateDefault(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty", nameof(userId));
        
        return new Wishlist(userId, "Default Wishlist", true);
    }
    
    public static Wishlist CreateCustom(Guid userId, string name)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty", nameof(userId));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Wishlist name cannot be empty", nameof(name));
        
        return new Wishlist(userId, name);
    }
    
    public void AddItem(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);
    
        if (_items.Any(i => i.ProductId == product.Id))
            throw new InvalidOperationException($"Product {product.Name} already in wishlist");
    
        var productImageUrl = product.ImageUrl;
    
        var item = WishlistItem.Create(Id, product.Id, product.Name, product.Price, productImageUrl);
        _items.Add(item);
        UpdatedAt = DateTime.UtcNow;
    
        AddDomainEvent(new WishlistItemAddedEvent(UserId, product.Id, product.Name));
    }
    
    public void RemoveItem(Guid productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
            throw new InvalidOperationException($"Product {productId} not in wishlist");
        
        _items.Remove(item);
        UpdatedAt = DateTime.UtcNow;
    }
    
    public bool Contains(Guid productId)
    {
        return _items.Any(i => i.ProductId == productId);
    }
    
    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Wishlist name cannot be empty", nameof(newName));
        
        Name = newName;
        UpdatedAt = DateTime.UtcNow;
    }
}