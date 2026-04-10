// Application/Features/Products/Queries/GetTotalStockValue/GetTotalStockValueQuery.cs

using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Features.Products.Models;

namespace NexoCommerceAI.Application.Features.Products.Queries;

[Cacheable("total_stock_value")]
public class GetTotalStockValueQuery : IRequest<TotalStockValueResponse>
{
}