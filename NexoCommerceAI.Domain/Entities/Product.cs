using NexoCommerceAI.Domain.Common;

namespace NexoCommerceAI.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public decimal? CompareAtPrice { get; private set; }
    public string Sku { get; private set; } = default!;
    public int Stock { get; private set; }
    public bool IsFeatured { get; private set; }
    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = default!;
    
    private Product() { }  // EF

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
    
    private static void ValidateProduct(string name, string slug, string? description, decimal price, 
                                        decimal? compareAtPrice, string sku, int stock)
    {
        if (string.IsNullOrWhiteSpace(name)) 
            throw new ArgumentException("Name is required");
            
        if (string.IsNullOrWhiteSpace(slug)) 
            throw new ArgumentException("Slug is required");
            
        if (description?.Length > 2000) 
            throw new ArgumentException("Description cannot exceed 2000 characters");
            
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU is required");
            
        if (sku.Length > 50)
            throw new ArgumentException("SKU cannot exceed 50 characters");
            
        if (price <= 0)
            throw new ArgumentException("Price must be greater than zero");
            
        if (price > 1_000_000)
            throw new ArgumentException("Price exceeds maximum allowed of 1,000,000");
            
        if (compareAtPrice.HasValue)
        {
            if (compareAtPrice.Value <= 0)
                throw new ArgumentException("Compare at price must be greater than zero");
                
            if (compareAtPrice.Value > 1_000_000)
                throw new ArgumentException("Compare at price exceeds maximum allowed of 1,000,000");
                
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
    
    public void Update(string name, string? slug = null, string? description = null, 
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
    
    public void UpdateSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug is required");
        
        Slug = SlugGenerator.Generate(slug);
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void UpdatePrice(decimal newPrice, decimal? newCompareAtPrice = null)
    {
        if (newPrice <= 0)
            throw new ArgumentException("Price must be greater than zero");
            
        if (newPrice > 1_000_000)
            throw new ArgumentException("Price exceeds maximum allowed");
            
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
            
        Stock += quantity;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0) 
            throw new ArgumentException("Quantity must be greater than zero");
            
        if (Stock < quantity) 
            throw new InvalidOperationException($"Not enough stock available. Current stock: {Stock}, requested: {quantity}");
            
        Stock -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }
    
    private static string GenerateSku(string name)
    {
        var prefix = string.Concat(name.Take(4)).ToUpper();
        // Usar Guid para mayor unicidad
        var uniqueId = Guid.NewGuid().ToString("N")[..8].ToUpper();
        return $"{prefix}-{uniqueId}";
    }
}