using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Features.Products.Models;

namespace NexoCommerceAI.Application.Features.Products.Commands;

[InvalidateCache("products_list", "featured_products", "products_on_sale", "product_by_id")]
public class UpdateProductCommand : IRequest<ProductResponse>
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public string? Slug { get; init; }
    public string? Description { get; init; }
    public decimal? Price { get; init; }
    public decimal? CompareAtPrice { get; init; }
    public string? Sku { get; init; }
    public int? Stock { get; init; }
    public bool? IsFeatured { get; init; }
    public Guid? CategoryId { get; init; }
}