namespace NexoCommerceAI.Application.Features.Products.Models;

public record ProductResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    decimal Price,
    decimal? CompareAtPrice,
    string Sku,
    int Stock,
    Guid CategoryId,
    string CategoryName,
    string CategorySlug,
    bool IsActive,
    bool IsFeatured,
    DateTime CreatedAt
);