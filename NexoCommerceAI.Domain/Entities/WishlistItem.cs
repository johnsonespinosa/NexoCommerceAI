using NexoCommerceAI.Domain.Common;

namespace NexoCommerceAI.Domain.Entities;

public class WishlistItem : BaseEntity
{
    public Guid WishlistId { get; private set; }
    public Wishlist Wishlist { get; private set; } = default!;
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = default!;
    public string ProductName { get; private set; } = default!;
    public decimal Price { get; private set; }
    public string? ProductImageUrl { get; private set; }
    public string? Notes { get; private set; }
    
    private WishlistItem() { }
    
    private WishlistItem(Guid wishlistId, Guid productId, string productName, decimal price, string? productImageUrl, string? notes = null)
    {
        WishlistId = wishlistId;
        ProductId = productId;
        ProductName = productName;
        Price = price;
        ProductImageUrl = productImageUrl;
        Notes = notes;
    }
    
    public static WishlistItem Create(Guid wishlistId, Guid productId, string productName, decimal price, string? productImageUrl, string? notes = null)
    {
        if (wishlistId == Guid.Empty)
            throw new ArgumentException("WishlistId cannot be empty", nameof(wishlistId));
        
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId cannot be empty", nameof(productId));
        
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("ProductName cannot be empty", nameof(productName));
        
        if (price <= 0)
            throw new ArgumentException("Price must be greater than zero", nameof(price));
        
        return new WishlistItem(wishlistId, productId, productName, price, productImageUrl, notes);
    }
    
    public void AddNotes(string notes)
    {
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }
}