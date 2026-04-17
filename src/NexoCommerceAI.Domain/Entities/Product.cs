using NexoCommerceAI.Domain.Common;
using NexoCommerceAI.Domain.Events;
using NexoCommerceAI.Domain.Utilities;

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
    public string? ImageUrl => GetMainImage()?.ImageUrl;

    private Product()
    {
        Name = string.Empty;
        Slug = string.Empty;
        Sku = string.Empty;
    }

    private Product(
        string name,
        string slug,
        string? description,
        decimal price,
        decimal? compareAtPrice,
        string sku,
        int stock,
        bool isFeatured,
        Guid categoryId)
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

    public static Product Create(
        string name,
        Guid categoryId,
        string? slug = null,
        string? description = null,
        decimal price = 0,
        decimal? compareAtPrice = null,
        string? sku = null,
        int stock = 0,
        bool isFeatured = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
        if (categoryId == Guid.Empty)
            throw new ArgumentException("CategoryId is required", nameof(categoryId));

        var finalSlug = string.IsNullOrWhiteSpace(slug)
            ? SlugGenerator.Generate(name)
            : SlugGenerator.Generate(slug);

        var finalSku = string.IsNullOrWhiteSpace(sku)
            ? GenerateSku(name)
            : sku.Trim();

        var finalName = name.Trim();
        var finalDescription = string.IsNullOrWhiteSpace(description) ? null : description.Trim();

        ValidateProduct(finalName, finalSlug, finalDescription, price, compareAtPrice, finalSku, stock);

        return new Product(finalName, finalSlug, finalDescription, price, compareAtPrice, finalSku, stock, isFeatured, categoryId);
    }

    public static Product CreateSimple(string name, string description, decimal price, int stock, Guid categoryId)
        => Create(name, categoryId, description: description, price: price, stock: stock);

    public void Update(
        string? name = null,
        string? slug = null,
        string? description = null,
        decimal? price = null,
        decimal? compareAtPrice = null,
        string? sku = null,
        int? stock = null,
        bool? isFeatured = null,
        Guid? categoryId = null)
    {
        var newName = string.IsNullOrWhiteSpace(name) ? Name : name.Trim();
        var newSlug = string.IsNullOrWhiteSpace(slug)
            ? (string.IsNullOrWhiteSpace(name) ? Slug : SlugGenerator.Generate(newName))
            : SlugGenerator.Generate(slug);
        var newDescription = description is null ? Description : (string.IsNullOrWhiteSpace(description) ? null : description.Trim());
        var newPrice = price ?? Price;
        var newCompareAtPrice = compareAtPrice ?? CompareAtPrice;
        var newSku = string.IsNullOrWhiteSpace(sku) ? Sku : sku.Trim();
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

    public void ClearCompareAtPrice()
    {
        CompareAtPrice = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateSlug(string? slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug is required", nameof(slug));
        Slug = SlugGenerator.Generate(slug);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePrice(decimal newPrice, decimal? newCompareAtPrice = null)
    {
        ValidatePrice(newPrice);
        ValidateCompareAtPrice(newPrice, newCompareAtPrice);
        Price = newPrice;
        CompareAtPrice = newCompareAtPrice;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStock(int newStock)
    {
        if (newStock < 0) throw new ArgumentException("Stock cannot be negative", nameof(newStock));
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
        if (!CompareAtPrice.HasValue || CompareAtPrice.Value <= Price) return 0;
        return Math.Round((CompareAtPrice.Value - Price) / CompareAtPrice.Value * 100, 2);
    }

    public bool IsOnSale() => CompareAtPrice > Price;
    public bool IsInStock() => Stock > 0;
    public bool IsLowStock(int threshold = 5) => Stock > 0 && Stock <= threshold;

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
        if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        var previousStock = Stock;
        Stock += quantity;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new StockRestockedEvent(Id, Name, previousStock, Stock, quantity));
    }

    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        if (Stock < quantity)
            throw new InvalidOperationException($"Not enough stock available. Current stock: {Stock}, requested: {quantity}");

        var previousStock = Stock;
        Stock -= quantity;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new StockChangedEvent(Id, Name, previousStock, Stock, -quantity));

        if (Stock == 0)
            AddDomainEvent(new OutOfStockEvent(Id, Name));
        else if (Stock <= 5)
            AddDomainEvent(new StockLowEvent(Id, Name, Stock, 5));
    }

    public void AddImage(ProductImage image)
    {
        ArgumentNullException.ThrowIfNull(image);

        if (Images.Count == 0 || image.IsMain)
        {
            var currentMain = Images.FirstOrDefault(i => i.IsMain);
            currentMain?.UnsetAsMain();
            image.SetAsMain();
        }

        Images.Add(image);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProductImageAddedEvent(Id, Name, image.Id, image.ImageUrl, image.IsMain));
    }

    public void RemoveImage(ProductImage image)
    {
        ArgumentNullException.ThrowIfNull(image);
        Images.Remove(image);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProductImageRemovedEvent(Id, Name, image.Id, image.ImageUrl, image.PublicId));
    }

    public void SetMainImage(Guid imageId)
    {
        var image = Images.FirstOrDefault(i => i.Id == imageId);
        if (image is null)
            throw new InvalidOperationException($"Image {imageId} not found");

        var currentMain = Images.FirstOrDefault(i => i.IsMain);
        currentMain?.UnsetAsMain();
        image.SetAsMain();
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProductImageSetMainEvent(Id, Name, image.Id, image.ImageUrl, true));
    }

    public ProductImage? GetMainImage() => Images.FirstOrDefault(productImage => productImage.IsMain);

    public IReadOnlyList<ProductImage> GetImagesOrdered() =>
        Images.OrderByDescending(i => i.IsMain).ThenBy(i => i.DisplayOrder).ToList();

    private static void ValidateProduct(
        string name,
        string slug,
        string? description,
        decimal price,
        decimal? compareAtPrice,
        string sku,
        int stock)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug is required", nameof(slug));
        if (description?.Length > 2000)
            throw new ArgumentException("Description cannot exceed 2000 characters", nameof(description));
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU is required", nameof(sku));
        if (sku.Length > 50)
            throw new ArgumentException("SKU cannot exceed 50 characters", nameof(sku));

        ValidatePrice(price);
        ValidateCompareAtPrice(price, compareAtPrice);

        if (stock < 0)
            throw new ArgumentException("Stock cannot be negative", nameof(stock));
    }

    private static void ValidatePrice(decimal price)
    {
        if (price <= 0) throw new ArgumentException("Price must be greater than zero", nameof(price));
        if (price > 1_000_000) throw new ArgumentException("Price exceeds maximum allowed of 1,000,000", nameof(price));
    }

    private static void ValidateCompareAtPrice(decimal price, decimal? compareAtPrice)
    {
        if (!compareAtPrice.HasValue) return;
        if (compareAtPrice.Value <= 0)
            throw new ArgumentException("Compare at price must be greater than zero", nameof(compareAtPrice));
        if (compareAtPrice.Value > 1_000_000)
            throw new ArgumentException("Compare at price exceeds maximum allowed of 1,000,000", nameof(compareAtPrice));
        if (compareAtPrice.Value <= price)
            throw new ArgumentException("Compare at price must be greater than regular price", nameof(compareAtPrice));
    }

    private static string GenerateSku(string name)
    {
        var clean = string.Join(string.Empty, name.Where(char.IsLetterOrDigit));
        var prefix = clean.Length >= 4 ? clean[..4] : clean.PadRight(4, 'X');
        var uniqueId = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        return $"{prefix.ToUpperInvariant()}-{uniqueId}";
    }
}
