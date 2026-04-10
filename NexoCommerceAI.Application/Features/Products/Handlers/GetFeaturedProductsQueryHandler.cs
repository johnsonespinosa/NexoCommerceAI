using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Extensions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Models;
using NexoCommerceAI.Application.Features.Products.Queries;

namespace NexoCommerceAI.Application.Features.Products.Handlers;

public class GetFeaturedProductsQueryHandler(
    IProductRepository productRepository,
    ILogger<GetFeaturedProductsQueryHandler> logger)
    : IRequestHandler<GetFeaturedProductsQuery, IReadOnlyList<ProductResponse>>
{
    public async Task<IReadOnlyList<ProductResponse>> Handle(GetFeaturedProductsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            logger.GetFeaturedProductsStarted(request.Take);
            
            var products = await productRepository.GetFeaturedProductsAsync(request.Take, cancellationToken);
            
            logger.FeaturedProductsFoundCount(products.Count);
            
            var items = products.Select(p => new ProductResponse(p, p.Category)).ToList();
            
            return items;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting featured products");
            throw;
        }
    }
}