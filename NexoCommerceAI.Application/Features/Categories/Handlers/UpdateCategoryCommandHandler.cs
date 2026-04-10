using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Categories.Commands;
using NexoCommerceAI.Application.Features.Categories.Models;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Categories.Handlers;

public class UpdateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    ILogger<UpdateCategoryCommandHandler> logger)
    : IRequestHandler<UpdateCategoryCommand, CategoryResponse>
{
    public async Task<CategoryResponse> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Updating category: {CategoryId}", request.Id);
            
            var category = await categoryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (category == null)
                throw new NotFoundException(nameof(Category), request.Id);
            
            // Preparar delegados para validación de unicidad usando el repositorio
            Func<string, Task<bool>>? isNameUnique = null;
            Func<string, Task<bool>>? isSlugUnique = null;
            
            if (request.Name != null && request.Name != category.Name)
            {
                isNameUnique = async name => 
                    await categoryRepository.IsNameUniqueAsync(name, request.Id, cancellationToken);
            }
            
            if (request.Slug != null && request.Slug != category.Slug)
            {
                isSlugUnique = async slug => 
                    await categoryRepository.IsSlugUniqueAsync(slug, request.Id, cancellationToken);
            }
            
            category.Update(request.Name, request.Slug, isNameUnique, isSlugUnique);
            
            await categoryRepository.UpdateAsync(category, cancellationToken);
            await categoryRepository.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("Category updated successfully: {CategoryId} - {CategoryName}", 
                category.Id, category.Name);
            
            return new CategoryResponse(category);
        }
        catch (ArgumentException ex)
        {
            logger.LogError(ex, "Invalid category data: {Message}", ex.Message);
            throw new ValidationException(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error updating category: {CategoryId}", request.Id);
            throw;
        }
    }
}