// Application/Features/Products/Commands/BulkUpdateStock/BulkUpdateStockCommand.cs

using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Features.Products.Models;

namespace NexoCommerceAI.Application.Features.Products.Commands;

[InvalidateCache("products_list", "featured_products", "products_on_sale", "low_stock_products", "product_by_id", "products_by_category")]
public class BulkUpdateStockCommand(IReadOnlyList<BulkStockUpdateItem> items) : IRequest<BulkUpdateStockResult>
{
    public IReadOnlyList<BulkStockUpdateItem> Items { get; init; } = items;
}