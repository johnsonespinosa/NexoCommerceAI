using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Features.Products.Models;

namespace NexoCommerceAI.Application.Features.Products.Queries;

[Cacheable("product_images")]
public class GetProductImagesQuery(Guid productId) : IRequest<IReadOnlyList<ProductImageResponse>>
{
    public Guid ProductId { get; set; } = productId;
}