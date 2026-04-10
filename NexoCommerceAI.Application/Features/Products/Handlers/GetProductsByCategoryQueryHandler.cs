using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Extensions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Models;
using NexoCommerceAI.Application.Features.Products.Queries;

namespace NexoCommerceAI.Application.Features.Products.Handlers;

public class GetProductsByCategoryQueryHandler(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    ILogger<GetProductsByCategoryQueryHandler> logger)
    : IRequestHandler<GetProductsByCategoryQuery, IReadOnlyList<ProductResponse>>
{
    public async Task<IReadOnlyList<ProductResponse>> Handle(GetProductsByCategoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            logger.GetProductsByCategoryStarted(request.CategoryId, request.Take?.ToString() ?? "all");
            
            // Verificar que la categoría existe
            var category = await categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category == null || category.IsDeleted || !category.IsActive)
            {
                logger.CategoryNotFoundOrInactive(request.CategoryId);
                return new List<ProductResponse>();
            }
            
            var products = await productRepository.GetProductsByCategoryAsync(request.CategoryId, cancellationToken);
            
            var originalCount = products.Count;
            
            // Aplicar límite si se especificó
            if (request.Take.HasValue && products.Count > request.Take.Value)
            {
                products = products.Take(request.Take.Value).ToList();
                logger.ProductsByCategoryLimited(originalCount, request.Take.Value);
            }
            
            logger.ProductsByCategoryFoundCount(products.Count, category.Name);
            
            var items = products.Select(p => new ProductResponse(p, p.Category)).ToList();
            
            return items;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting products by category: {CategoryId}", request.CategoryId);
            throw;
        }
    }
}