using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Categories.Commands;
using NexoCommerceAI.Application.Features.Categories.Models;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Categories.Handlers;

public class CreateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    ILogger<CreateCategoryCommandHandler> logger)
    : IRequestHandler<CreateCategoryCommand, CategoryResponse>
{
    public async Task<CategoryResponse> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Creating category: {CategoryName}", request.Name);
            
            // Verificar unicidad de slug usando el repositorio
            var isSlugUnique = async (string slug) => await categoryRepository.IsSlugUniqueAsync(slug, cancellationToken);
            
            var category = Category.Create(request.Name, request.Slug, isSlugUnique);
            
            await categoryRepository.AddAsync(category, cancellationToken);
            await categoryRepository.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("Category created successfully: {CategoryId} - {CategoryName}", 
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
            logger.LogError(ex, "Unexpected error creating category: {CategoryName}", request.Name);
            throw;
        }
    }
}