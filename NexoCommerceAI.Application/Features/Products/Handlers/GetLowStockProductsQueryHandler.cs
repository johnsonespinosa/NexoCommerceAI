using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Extensions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Models;
using NexoCommerceAI.Application.Features.Products.Queries;

namespace NexoCommerceAI.Application.Features.Products.Handlers;

public class GetLowStockProductsQueryHandler(
    IProductRepository productRepository,
    ILogger<GetLowStockProductsQueryHandler> logger)
    : IRequestHandler<GetLowStockProductsQuery, IReadOnlyList<ProductResponse>>
{
    public async Task<IReadOnlyList<ProductResponse>> Handle(GetLowStockProductsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            logger.GetLowStockProductsStarted(request.Threshold);
            
            var products = await productRepository.GetLowStockProductsAsync(request.Threshold, cancellationToken);
            
            logger.LowStockProductsFoundCount(products.Count, request.Threshold);
            
            var items = products.Select(p => new ProductResponse(p, p.Category)).ToList();
            
            return items;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting low stock products with threshold: {Threshold}", request.Threshold);
            throw;
        }
    }
}