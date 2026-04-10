using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Extensions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Common.Models;
using NexoCommerceAI.Application.Features.Products.Models;
using NexoCommerceAI.Application.Features.Products.Queries;

namespace NexoCommerceAI.Application.Features.Products.Handlers;

public class GetProductsListQueryHandler(
    IProductRepository productRepository,
    ILogger<GetProductsListQueryHandler> logger)
    : IRequestHandler<GetProductsListQuery, PaginatedResult<ProductResponse>>
{
    public async Task<PaginatedResult<ProductResponse>> Handle(GetProductsListQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var pagination = request.Pagination;
            
            logger.GetProductsListStarted(
                pagination.PageNumber,
                pagination.PageSize,
                pagination.SearchTerm ?? "none",
                pagination.CategoryId?.ToString() ?? "all");
            
            // Usar el método del repositorio con todos los filtros
            var (products, totalCount) = await productRepository.GetPaginatedWithFiltersAsync(
                pagination.PageNumber,
                pagination.PageSize,
                pagination.SearchTerm,
                pagination.CategoryId,
                pagination.IsFeatured,
                pagination.IsOnSale,
                pagination.MinPrice,
                pagination.MaxPrice,
                pagination.SortBy,
                pagination.SortDescending,
                cancellationToken);
            
            logger.ProductsFoundCount(products.Count, totalCount);
            
            // Mapear a respuesta (los productos ya incluyen la categoría por el Include)
            var items = products.Select(p => new ProductResponse(p, p.Category)).ToList();
            
            var result = new PaginatedResult<ProductResponse>(
                items,
                totalCount,
                pagination.PageNumber,
                pagination.PageSize);
            
            logger.GetProductsListCompleted(
                result.PageNumber,
                result.TotalPages,
                result.Items.Count,
                result.TotalCount);
            
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting products list");
            throw;
        }
    }
}