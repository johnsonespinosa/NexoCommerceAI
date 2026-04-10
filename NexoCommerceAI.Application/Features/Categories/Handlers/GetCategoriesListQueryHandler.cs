using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Common.Models;
using NexoCommerceAI.Application.Features.Categories.Models;
using NexoCommerceAI.Application.Features.Categories.Queries;

namespace NexoCommerceAI.Application.Features.Categories.Handlers;

public class GetCategoriesListQueryHandler(
    ICategoryRepository categoryRepository,
    ILogger<GetCategoriesListQueryHandler> logger)
    : IRequestHandler<GetCategoriesListQuery, PaginatedResult<CategoryResponse>>
{
    public async Task<PaginatedResult<CategoryResponse>> Handle(GetCategoriesListQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var pagination = request.Pagination;
            
            logger.LogInformation("Getting categories list - Page: {PageNumber}, Size: {PageSize}, Search: {SearchTerm}", 
                pagination.PageNumber, pagination.PageSize, pagination.SearchTerm ?? "none");
            
            // Obtener todas las categorías no eliminadas
            var allCategories = await categoryRepository.ListAsync(cancellationToken);
            var activeCategories = allCategories.Where(c => !c.IsDeleted).ToList();
            
            // Aplicar búsqueda
            if (!string.IsNullOrWhiteSpace(pagination.SearchTerm))
            {
                var searchTerm = pagination.SearchTerm.ToLower();
                activeCategories = activeCategories
                    .Where(c => c.Name.ToLower().Contains(searchTerm) || c.Slug.Contains(searchTerm))
                    .ToList();
            }
            
            var totalCount = activeCategories.Count;
            
            // Aplicar ordenamiento
            if (!string.IsNullOrWhiteSpace(pagination.SortBy))
            {
                var sortBy = pagination.SortBy.ToLower();
                if (sortBy == "name")
                {
                    activeCategories = pagination.SortDescending
                        ? activeCategories.OrderByDescending(c => c.Name).ToList()
                        : activeCategories.OrderBy(c => c.Name).ToList();
                }
                else
                {
                    activeCategories = pagination.SortDescending
                        ? activeCategories.OrderByDescending(c => c.CreatedAt).ToList()
                        : activeCategories.OrderBy(c => c.CreatedAt).ToList();
                }
            }
            else
            {
                activeCategories = activeCategories.OrderBy(c => c.Name).ToList();
            }
            
            // Aplicar paginación
            var items = activeCategories
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(c => new CategoryResponse(c, c.GetProductCount()))
                .ToList();
            
            logger.LogDebug("Found {TotalCount} categories, returning {ItemCount} items", totalCount, items.Count);
            
            var result = new PaginatedResult<CategoryResponse>(
                items,
                totalCount,
                pagination.PageNumber,
                pagination.PageSize);
            
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting categories list");
            throw;
        }
    }
}