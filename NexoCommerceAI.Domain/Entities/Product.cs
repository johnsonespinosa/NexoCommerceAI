using NexoCommerceAI.Domain.Common;
using NexoCommerceAI.Domain.Events;

namespace NexoCommerceAI.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; private set; }
    public string Slug { get; private set; }
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public decimal? CompareAtPrice { get; private set; }
    public string Sku { get; private set; }
    public int Stock { get; private set; }
    public bool IsFeatured { get; private set; }
    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = default!;
    public ICollection<ProductImage> Images { get; private set; } = new List<ProductImage>();

    private Product(string name, string slug, string? description, decimal price, decimal? compareAtPrice, 
                    string sku, int stock, bool isFeatured, Guid categoryId)
    {
        Name = name;
        Slug = slug;
        Description = description;
        Price = price;
        CompareAtPrice = compareAtPrice;
        Sku = sku;
        Stock = stock;
        IsFeatured = isFeatured;
        CategoryId = categoryId;
    }
    
    private static void ValidateProduct(
        string? name, 
        string slug, 
        string? description, 
        decimal price, 
        decimal? compareAtPrice, 
        string sku, 
        int stock)
    {
        _ = name ?? throw new ArgumentException("Name is required");
        _ = slug ?? throw new ArgumentException("Slug is required");
        
        if (description?.Length > 2000) 
            throw new ArgumentException("Description cannot exceed 2000 characters");
        
        _ = sku ?? throw new ArgumentException("SKU is required");
        
        if (sku.Length > 50)
            throw new ArgumentException("SKU cannot exceed 50 characters");
        
        switch (price)
        {
            case <= 0:
                throw new ArgumentException("Price must be greater than zero");
            case > 1_000_000:
                throw new ArgumentException("Price exceeds maximum allowed of 1,000,000");
        }

        if (compareAtPrice.HasValue)
        {
            switch (compareAtPrice.Value)
            {
                case <= 0:
                    throw new ArgumentException("Compare at price must be greater than zero");
                case > 1_000_000:
                    throw new ArgumentException("Compare at price exceeds maximum allowed of 1,000,000");
            }

            if (compareAtPrice.Value <= price)
                throw new ArgumentException("Compare at price must be greater than regular price");
        }

        if (stock < 0) 
            throw new ArgumentException("Stock cannot be negative");
    }
    
    public static Product Create(string name, string? slug = null, string? description = null, 
                                 decimal price = 0, decimal? compareAtPrice = null, 
                                 string? sku = null, int stock = 0, bool isFeatured = false, 
                                 Guid? categoryId = null)
    {
        // Generar slug si no se proporciona
        var finalSlug = string.IsNullOrWhiteSpace(slug) 
            ? SlugGenerator.Generate(name) 
            : SlugGenerator.Generate(slug);
        
        // Generar SKU si no se proporciona
        var finalSku = string.IsNullOrWhiteSpace(sku) 
            ? GenerateSku(name) 
            : sku;
        
        if (categoryId == null || categoryId == Guid.Empty)
            throw new ArgumentException("CategoryId is required");
        
        ValidateProduct(name, finalSlug, description, price, compareAtPrice, finalSku, stock);
        
        return new Product(name, finalSlug, description, price, compareAtPrice, 
                          finalSku, stock, isFeatured, categoryId.Value);
    }
    
    // Método principal con parámetros opcionales pero orden lógico
    public static Product Create(string name, Guid categoryId, string? slug = null, 
        string? description = null, decimal price = 0, 
        decimal? compareAtPrice = null, string? sku = null, 
        int stock = 0, bool isFeatured = false)
    {
        // Generar slug si no se proporciona
        var finalSlug = string.IsNullOrWhiteSpace(slug) 
            ? SlugGenerator.Generate(name) 
            : SlugGenerator.Generate(slug);
        
        // Generar SKU si no se proporciona
        var finalSku = string.IsNullOrWhiteSpace(sku) 
            ? GenerateSku(name) 
            : sku;
        
        if (categoryId == Guid.Empty)
            throw new ArgumentException("CategoryId is required");
        
        ValidateProduct(name, finalSlug, description, price, compareAtPrice, finalSku, stock);
        
        return new Product(name, finalSlug, description, price, compareAtPrice, 
            finalSku, stock, isFeatured, categoryId);
    }
    
    // Overload simplificado para casos comunes
    public static Product CreateSimple(string name, string description, decimal price, 
        int stock, Guid categoryId)
    {
        return Create(name, categoryId, description: description, price: price, stock: stock);
    }
    
    public void Update(string? name, string? slug = null, string? description = null, 
                      decimal? price = null, decimal? compareAtPrice = null, 
                      string? sku = null, int? stock = null, bool? isFeatured = null, 
                      Guid? categoryId = null)
    {
        var newName = name ?? Name;
        var newSlug = string.IsNullOrWhiteSpace(slug) 
            ? name != null ? SlugGenerator.Generate(name) : Slug 
            : SlugGenerator.Generate(slug);
        
        var newDescription = description ?? Description;
        var newPrice = price ?? Price;
        var newCompareAtPrice = compareAtPrice ?? CompareAtPrice;
        var newSku = sku ?? Sku;
        var newStock = stock ?? Stock;
        var newIsFeatured = isFeatured ?? IsFeatured;
        var newCategoryId = categoryId ?? CategoryId;
        
        ValidateProduct(newName, newSlug, newDescription, newPrice, newCompareAtPrice, newSku, newStock);
        
        Name = newName;
        Slug = newSlug;
        Description = newDescription;
        Price = newPrice;
        CompareAtPrice = newCompareAtPrice;
        Sku = newSku;
        Stock = newStock;
        IsFeatured = newIsFeatured;
        CategoryId = newCategoryId;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void UpdateSlug(string? slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug is required");
        
        Slug = SlugGenerator.Generate(slug);
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void UpdatePrice(decimal newPrice, decimal? newCompareAtPrice = null)
    {
        switch (newPrice)
        {
            case <= 0:
                throw new ArgumentException("Price must be greater than zero");
            case > 1_000_000:
                throw new ArgumentException("Price exceeds maximum allowed");
        }

        if (newCompareAtPrice.HasValue)
        {
            if (newCompareAtPrice.Value <= 0)
                throw new ArgumentException("Compare at price must be greater than zero");
                
            if (newCompareAtPrice.Value <= newPrice)
                throw new ArgumentException("Compare at price must be greater than regular price");
        }
        
        Price = newPrice;
        CompareAtPrice = newCompareAtPrice;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void UpdateStock(int newStock)
    {
        if (newStock < 0)
            throw new ArgumentException("Stock cannot be negative");
            
        Stock = newStock;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void MarkAsFeatured()
    {
        IsFeatured = true;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void UnmarkAsFeatured()
    {
        IsFeatured = false;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public decimal GetDiscountPercentage()
    {
        if (!CompareAtPrice.HasValue || CompareAtPrice.Value <= Price)
            return 0;
            
        return Math.Round((CompareAtPrice.Value - Price) / CompareAtPrice.Value * 100, 2);
    }
    
    public bool IsOnSale()
    {
        return CompareAtPrice > Price;
    }
    
    public bool IsInStock()
    {
        return Stock > 0;
    }
    
    public bool IsLowStock(int threshold = 5)
    {
        return Stock > 0 && Stock <= threshold;
    }
    
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void SoftDelete()
    {
        IsDeleted = true;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0) 
            throw new ArgumentException("Quantity must be greater than zero");
            
        var previousStock = Stock;
        Stock += quantity;
        UpdatedAt = DateTime.UtcNow;
        
        // Agregar evento de dominio para restock
        AddDomainEvent(new StockRestockedEvent
        (
            ProductId: Id,
            ProductName: Name,
            PreviousStock: previousStock,
            NewStock: Stock,
            QuantityAdded: quantity
        ));
    }
    
    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0) 
            throw new ArgumentException("Quantity must be greater than zero");
        
        if (Stock < quantity) 
            throw new InvalidOperationException($"Not enough stock available. Current stock: {Stock}, requested: {quantity}");
        
        var previousStock = Stock;
        Stock -= quantity;
        UpdatedAt = DateTime.UtcNow;

        // Agregar evento de cambio de stock
        AddDomainEvent(new StockChangedEvent(
            ProductId: Id,
            ProductName: Name,
            PreviousStock: previousStock,
            NewStock: Stock,
            QuantityChanged: -quantity
        ));

        switch (Stock)
        {
            case 0:
                AddDomainEvent(new OutOfStockEvent(
                    ProductId: Id,
                    ProductName: Name
                ));
                break;
            case <= 5:
                AddDomainEvent(new StockLowEvent
                (
                    ProductId: Id,
                    ProductName: Name,
                    CurrentStock: Stock,
                    Threshold: 5
                ));
                break;
        }
    }
    
    public void AddImage(ProductImage image)
    {
        ArgumentNullException.ThrowIfNull(image);
        
        // Si es la primera imagen o se marca como principal
        if (Images.Count == 0 || image.IsMain)
        {
            // Si hay una imagen principal actual, la desmarcamos
            var currentMain = Images.FirstOrDefault(i => i.IsMain);
            currentMain?.UnsetAsMain();
        }
        
        Images.Add(image);
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new ProductImageAddedEvent(
            Id,
            Name,
            image.Id,
            image.ImageUrl,
            image.IsMain));
    }
    
    public void RemoveImage(ProductImage image)
    {
        ArgumentNullException.ThrowIfNull(image);
        
        Images.Remove(image);
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new ProductImageRemovedEvent(
            Id,
            Name,
            image.Id,
            image.ImageUrl,
            image.PublicId));
    }
    
    public void SetMainImage(Guid imageId)
    {
        var image = Images.FirstOrDefault(i => i.Id == imageId);
        if (image == null)
            throw new InvalidOperationException($"Image {imageId} not found");
    
        var currentMain = Images.FirstOrDefault(i => i.IsMain);
        currentMain?.UnsetAsMain();

        image.SetAsMain();
        UpdatedAt = DateTime.UtcNow;
    
        AddDomainEvent(new ProductImageSetMainEvent(
            Id,
            Name,
            image.Id,
            image.ImageUrl,
            true));
    }
    
    public ProductImage? GetMainImage()
    {
        return Images.FirstOrDefault(i => i.IsMain);
    }
    
    public IReadOnlyList<ProductImage> GetImagesOrdered()
    {
        return Images.OrderBy(i => i.DisplayOrder).ThenByDescending(i => i.IsMain).ToList();
    }
    
    private static string GenerateSku(string? name)
    {
        var prefix = string.Concat(name!.Take(4)).ToUpper();
        // Usar Guid para mayor unicidad
        var uniqueId = Guid.NewGuid().ToString("N")[..8].ToUpper();
        return $"{prefix}-{uniqueId}";
    }
}