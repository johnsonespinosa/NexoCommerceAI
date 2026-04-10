namespace NexoCommerceAI.Application.Features.Products.Models;

public record ProductListItemDto(
    Guid Id,
    string Name,
    string Slug,
    decimal Price,
    int Stock,
    string CategoryName,
    bool IsActive
);