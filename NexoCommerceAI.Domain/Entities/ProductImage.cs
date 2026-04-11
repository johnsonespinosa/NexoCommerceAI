using NexoCommerceAI.Domain.Common;

namespace NexoCommerceAI.Domain.Entities;

public class ProductImage : BaseEntity
{
    // Properties con setters privados
    public Guid ProductId { get; private set; }
    public string ImageUrl { get; private set; }
    public string PublicId { get; private set; }
    public bool IsMain { get; private set; }
    public int DisplayOrder { get; private set; }
    
    // Navigation property
    public Product Product { get; private set; } = default!;
    
    // Constructor privado para fábrica
    private ProductImage(
        Guid productId,
        string imageUrl,
        string publicId,
        bool isMain,
        int displayOrder)
    {
        ProductId = productId;
        ImageUrl = imageUrl;
        PublicId = publicId;
        IsMain = isMain;
        DisplayOrder = displayOrder;
    }
    
    // Factory method
    public static ProductImage Create(
        Guid productId,
        string imageUrl,
        string publicId,
        bool isMain = false,
        int? displayOrder = null)
    {
        // Validaciones
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId cannot be empty", nameof(productId));
        
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("ImageUrl cannot be empty", nameof(imageUrl));
        
        if (string.IsNullOrWhiteSpace(publicId))
            throw new ArgumentException("PublicId cannot be empty", nameof(publicId));
        
        // Validar formato de URL
        if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
            throw new ArgumentException("Invalid URL format", nameof(imageUrl));
        
        var finalDisplayOrder = displayOrder ?? 0;
        
        return new ProductImage(
            productId,
            imageUrl,
            publicId,
            isMain,
            finalDisplayOrder);
    }
    
    // Métodos de negocio
    public void SetAsMain()
    {
        IsMain = true;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void UnsetAsMain()
    {
        IsMain = false;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void UpdateDisplayOrder(int newOrder)
    {
        if (newOrder < 0)
            throw new ArgumentException("Display order cannot be negative", nameof(newOrder));
        
        DisplayOrder = newOrder;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void UpdateUrl(string newImageUrl, string newPublicId)
    {
        if (string.IsNullOrWhiteSpace(newImageUrl))
            throw new ArgumentException("ImageUrl cannot be empty", nameof(newImageUrl));
        
        if (string.IsNullOrWhiteSpace(newPublicId))
            throw new ArgumentException("PublicId cannot be empty", nameof(newPublicId));
        
        if (!Uri.IsWellFormedUriString(newImageUrl, UriKind.Absolute))
            throw new ArgumentException("Invalid URL format", nameof(newImageUrl));
        
        ImageUrl = newImageUrl;
        PublicId = newPublicId;
        UpdatedAt = DateTime.UtcNow;
    }
}