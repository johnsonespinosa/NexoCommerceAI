// Application/Features/Categories/Queries/GetCategoryById/GetCategoryByIdQueryHandler.cs

using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Categories.Models;
using NexoCommerceAI.Application.Features.Categories.Queries;

namespace NexoCommerceAI.Application.Features.Categories.Handlers;

public class GetCategoryByIdQueryHandler(
    ICategoryRepository categoryRepository,
    ILogger<GetCategoryByIdQueryHandler> logger)
    : IRequestHandler<GetCategoryByIdQuery, CategoryResponse?>
{
    public async Task<CategoryResponse?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Getting category by ID: {CategoryId}", request.Id);
            
            var category = await categoryRepository.GetByIdAsync(request.Id, cancellationToken);
            
            if (category == null || category.IsDeleted)
            {
                logger.LogWarning("Category not found: {CategoryId}", request.Id);
                return null;
            }
            
            var productCount = category.GetProductCount();
            
            logger.LogDebug("Category found: {CategoryId} - {CategoryName}, Products: {ProductCount}", 
                category.Id, category.Name, productCount);
            
            return new CategoryResponse(category, productCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting category by ID: {CategoryId}", request.Id);
            throw;
        }
    }
}