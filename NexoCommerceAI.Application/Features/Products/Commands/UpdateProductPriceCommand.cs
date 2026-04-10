using MediatR;
using NexoCommerceAI.Application.Common.Attributes;

namespace NexoCommerceAI.Application.Features.Products.Commands;

[InvalidateCache("products_list", "featured_products", "products_on_sale", "product_by_id")]
public class UpdateProductPriceCommand : IRequest<bool>
{
    public Guid ProductId { get; init; }
    public decimal NewPrice { get; init; }
    public decimal? NewCompareAtPrice { get; init; }
}