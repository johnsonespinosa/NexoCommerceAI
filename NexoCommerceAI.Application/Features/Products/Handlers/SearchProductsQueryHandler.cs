using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Extensions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Models;
using NexoCommerceAI.Application.Features.Products.Queries;

namespace NexoCommerceAI.Application.Features.Products.Handlers;

public class SearchProductsQueryHandler(
    IProductRepository productRepository,
    ILogger<SearchProductsQueryHandler> logger)
    : IRequestHandler<SearchProductsQuery, IReadOnlyList<ProductResponse>>
{
    public async Task<IReadOnlyList<ProductResponse>> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            logger.SearchProductsStarted(request.SearchTerm, request.Take);
            
            var products = await productRepository.SearchProductsAsync(request.SearchTerm, request.Take, cancellationToken);
            
            logger.SearchProductsFoundCount(products.Count, request.SearchTerm);
            
            var items = products.Select(p => new ProductResponse(p, p.Category)).ToList();
            
            return items;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching products with term: {SearchTerm}", request.SearchTerm);
            throw;
        }
    }
}