using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Products.Models;

public record ProductResponse
{
    public Guid Id { get; init; }
    public string? Name { get; init; } = default!;
    public string Slug { get; init; } = default!;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public decimal? CompareAtPrice { get; init; }
    public string Sku { get; init; } = default!;
    public int Stock { get; init; }
    public bool IsFeatured { get; init; }
    public bool IsActive { get; init; }
    public Guid CategoryId { get; init; }
    public string CategoryName { get; init; } = default!;
    public string CategorySlug { get; init; } = default!;
    public decimal? DiscountPercentage { get; init; }
    public bool IsOnSale { get; init; }
    public bool IsInStock { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    
    // Constructor para mapeo explícito
    public ProductResponse()
    {
    }
    
    // Constructor para crear desde entidad
    public ProductResponse(Product product, Category? category = null)
    {
        Id = product.Id;
        Name = product.Name;
        Slug = product.Slug;
        Description = product.Description;
        Price = product.Price;
        CompareAtPrice = product.CompareAtPrice;
        Sku = product.Sku;
        Stock = product.Stock;
        IsFeatured = product.IsFeatured;
        IsActive = product.IsActive;
        CategoryId = product.CategoryId;
        CategoryName = category?.Name ?? string.Empty;
        CategorySlug = category?.Slug ?? string.Empty;
        DiscountPercentage = product.GetDiscountPercentage();
        IsOnSale = product.IsOnSale();
        IsInStock = product.IsInStock();
        CreatedAt = product.CreatedAt;
        UpdatedAt = product.UpdatedAt;
    }
}