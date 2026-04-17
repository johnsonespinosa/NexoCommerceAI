using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Products;

public sealed record ProductResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    decimal Price,
    decimal? CompareAtPrice,
    string Sku,
    int Stock,
    bool IsFeatured,
    Guid CategoryId,
    string? ImageUrl);

internal static class ProductMappings
{
    public static ProductResponse ToResponse(this Product product) =>
        new(
            Id: product.Id,
            Name: product.Name,
            Slug: product.Slug,
            Description: product.Description,
            Price: product.Price,
            CompareAtPrice: product.CompareAtPrice,
            Sku: product.Sku,
            Stock: product.Stock,
            IsFeatured: product.IsFeatured,
            CategoryId: product.CategoryId,
            ImageUrl: product.ImageUrl);
}
