using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Categories.Models;
using NexoCommerceAI.Application.Features.Categories.Queries;

namespace NexoCommerceAI.Application.Features.Categories.Handlers;

public class GetCategoryBySlugQueryHandler(
    ICategoryRepository categoryRepository,
    ILogger<GetCategoryBySlugQueryHandler> logger)
    : IRequestHandler<GetCategoryBySlugQuery, CategoryResponse?>
{
    public async Task<CategoryResponse?> Handle(GetCategoryBySlugQuery request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Getting category by slug: {Slug}", request.Slug);
            
            var category = await categoryRepository.GetBySlugAsync(request.Slug, cancellationToken);
            
            if (category == null || category.IsDeleted)
            {
                logger.LogWarning("Category not found with slug: {Slug}", request.Slug);
                return null;
            }
            
            var productCount = category.GetProductCount();
            
            logger.LogDebug("Category found: {Slug} - {CategoryId} - {CategoryName}, Products: {ProductCount}", 
                request.Slug, category.Id, category.Name, productCount);
            
            return new CategoryResponse(category, productCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting category by slug: {Slug}", request.Slug);
            throw;
        }
    }
}