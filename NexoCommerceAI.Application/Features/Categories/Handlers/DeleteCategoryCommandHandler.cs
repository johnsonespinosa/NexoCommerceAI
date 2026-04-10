using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Categories.Commands;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Categories.Handlers;

public class DeleteCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    ILogger<DeleteCategoryCommandHandler> logger)
    : IRequestHandler<DeleteCategoryCommand, bool>
{
    public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Deleting category: {CategoryId}", request.Id);
            
            var category = await categoryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (category == null)
            {
                logger.LogWarning("Category not found: {CategoryId}", request.Id);
                throw new NotFoundException(nameof(Category), request.Id);
            }
            
            if (category.IsDeleted)
            {
                logger.LogWarning("Category already deleted: {CategoryId} - {CategoryName}", 
                    category.Id, category.Name);
                return false;
            }
            
            // Verificar si la categoría tiene productos
            var productCount = category.GetProductCount();
            if (productCount > 0)
            {
                logger.LogWarning("Cannot delete category with products: {CategoryId} - {CategoryName}, Products: {ProductCount}", 
                    category.Id, category.Name, productCount);
                throw new ValidationException($"Cannot delete category '{category.Name}' because it has {productCount} products. Please reassign or delete the products first.");
            }
            
            category.SoftDelete();
            
            await categoryRepository.UpdateAsync(category, cancellationToken);
            await categoryRepository.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("Category deleted successfully: {CategoryId} - {CategoryName}", 
                category.Id, category.Name);
            
            return true;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error deleting category: {CategoryId}", request.Id);
            throw;
        }
    }
}