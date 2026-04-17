using NexoCommerceAI.Application.Abstractions.Messaging;
using NexoCommerceAI.Application.Common;

namespace NexoCommerceAI.Application.Products.CreateProduct;

public sealed record CreateProductCommand(
    string Name,
    Guid CategoryId,
    string? Description,
    decimal Price,
    decimal? CompareAtPrice,
    string? Sku,
    int Stock,
    bool IsFeatured
) : ICommand<CreateProductResult>;

public sealed record CreateProductResult(
    Guid Id,
    string Name,
    string Slug,
    string Sku,
    decimal Price,
    int Stock,
    Guid CategoryId
);
