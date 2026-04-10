using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Features.Products.Models;

namespace NexoCommerceAI.Application.Features.Products.Queries;

[Cacheable("product_by_slug")]
public class GetProductBySlugQuery(string slug) : IRequest<ProductResponse?>
{
    public string Slug { get; init; } = slug;
}