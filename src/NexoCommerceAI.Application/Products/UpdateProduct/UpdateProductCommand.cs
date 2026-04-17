using NexoCommerceAI.Application.Abstractions.Messaging;

namespace NexoCommerceAI.Application.Products.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid Id,
    string? Name,
    Guid? CategoryId,
    string? Description,
    decimal? Price,
    decimal? CompareAtPrice,
    string? Sku,
    int? Stock,
    bool? IsFeatured
) : ICommand<ProductResponse>;
