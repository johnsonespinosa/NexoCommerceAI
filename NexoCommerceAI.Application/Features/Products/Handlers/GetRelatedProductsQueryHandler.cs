using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Models;
using NexoCommerceAI.Application.Features.Products.Queries;

namespace NexoCommerceAI.Application.Features.Products.Handlers;

public class GetRelatedProductsQueryHandler(
    IProductRepository productRepository,
    ILogger<GetRelatedProductsQueryHandler> logger)
    : IRequestHandler<GetRelatedProductsQuery, IReadOnlyList<ProductResponse>>
{
    public async Task<IReadOnlyList<ProductResponse>> Handle(GetRelatedProductsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Getting related products for product: {ProductId}, Take: {Take}", 
                request.ProductId, request.Take);
            
            // Obtener el producto original con su categoría
            var product = await productRepository.GetByIdWithCategoryAsync(request.ProductId, cancellationToken);
            if (product == null)
            {
                logger.LogWarning("Product not found: {ProductId}", request.ProductId);
                return new List<ProductResponse>();
            }
            
            // Obtener productos relacionados de la misma categoría
            // Excluyendo el producto actual y asegurando que estén activos
            var relatedProducts = await productRepository.GetProductsByCategoryAsync(product.CategoryId, cancellationToken);
            
            var filteredProducts = relatedProducts
                .Where(p => p.Id != request.ProductId && p is { IsActive: true, IsDeleted: false } && p.IsInStock())
                .Take(request.Take)
                .ToList();
            
            logger.LogDebug("Found {Count} related products for product {ProductId} in category {CategoryName}", 
                filteredProducts.Count, request.ProductId, product.Category.Name);
            
            var items = filteredProducts.Select(p => new ProductResponse(p, p.Category)).ToList();
            
            return items;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting related products for product: {ProductId}", request.ProductId);
            throw;
        }
    }
}