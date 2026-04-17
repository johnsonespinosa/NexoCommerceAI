using NexoCommerceAI.Application.Abstractions.Messaging;

using NexoCommerceAI.Application.Common;

namespace NexoCommerceAI.Application.Products.GetProducts;

public sealed record GetProductsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    Guid? CategoryId = null,
    string? Sku = null,
    bool? IsFeatured = null)
    : IQuery<PaginatedResult<ProductResponse>>;
