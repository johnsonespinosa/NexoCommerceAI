using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Extensions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Models;
using NexoCommerceAI.Application.Features.Products.Queries;

namespace NexoCommerceAI.Application.Features.Products.Handlers;

public class GetProductsOnSaleQueryHandler(
    IProductRepository productRepository,
    ILogger<GetProductsOnSaleQueryHandler> logger)
    : IRequestHandler<GetProductsOnSaleQuery, IReadOnlyList<ProductResponse>>
{
    public async Task<IReadOnlyList<ProductResponse>> Handle(GetProductsOnSaleQuery request, CancellationToken cancellationToken)
    {
        try
        {
            logger.GetProductsOnSaleStarted(request.Take?.ToString() ?? "all");
            
            var products = await productRepository.GetProductsOnSaleAsync(cancellationToken);
            
            var originalCount = products.Count;
            
            // Aplicar límite si se especificó
            if (request.Take.HasValue && products.Count > request.Take.Value)
            {
                products = products.Take(request.Take.Value).ToList();
                logger.LogDebug("Limited products on sale from {OriginalCount} to {Take}", 
                    originalCount, request.Take.Value);
            }
            
            logger.ProductsOnSaleFoundCount(products.Count);
            
            var items = products.Select(p => new ProductResponse(p, p.Category)).ToList();
            
            return items;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting products on sale");
            throw;
        }
    }
}