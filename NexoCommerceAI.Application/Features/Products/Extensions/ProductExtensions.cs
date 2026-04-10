// Application/Features/Products/Extensions/ProductExtensions.cs
using NexoCommerceAI.Application.Features.Products.Models;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Products.Extensions;

public static class ProductExtensions
{
    public static ProductResponse ToResponse(this Product product, Category? category = null)
    {
        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            Description = product.Description,
            Price = product.Price,
            CompareAtPrice = product.CompareAtPrice,
            Sku = product.Sku,
            Stock = product.Stock,
            IsFeatured = product.IsFeatured,
            IsActive = product.IsActive,
            CategoryId = product.CategoryId,
            CategoryName = category?.Name ?? product.Category?.Name ?? string.Empty,
            CategorySlug = category?.Slug ?? product.Category?.Slug ?? string.Empty,
            DiscountPercentage = product.GetDiscountPercentage(),
            IsOnSale = product.IsOnSale(),
            IsInStock = product.IsInStock(),
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
    
    public static IReadOnlyList<ProductResponse> ToResponseList(this IEnumerable<Product> products)
    {
        return products.Select(p => p.ToResponse()).ToList();
    }
}