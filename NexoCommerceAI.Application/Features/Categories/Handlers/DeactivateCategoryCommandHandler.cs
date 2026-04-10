using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Categories.Commands;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Categories.Handlers;

public class DeactivateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    ILogger<DeactivateCategoryCommandHandler> logger)
    : IRequestHandler<DeactivateCategoryCommand, bool>
{
    public async Task<bool> Handle(DeactivateCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Deactivating category: {CategoryId}", request.Id);
            
            var category = await categoryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (category == null)
                throw new NotFoundException(nameof(Category), request.Id);
            
            if (category.IsDeleted)
            {
                logger.LogWarning("Cannot deactivate deleted category: {CategoryId} - {CategoryName}", 
                    category.Id, category.Name);
                throw new ValidationException($"Cannot deactivate deleted category '{category.Name}'");
            }
            
            if (!category.IsActive)
            {
                logger.LogInformation("Category already inactive: {CategoryId} - {CategoryName}", 
                    category.Id, category.Name);
                return false;
            }
            
            category.Deactivate();
            
            await categoryRepository.UpdateAsync(category, cancellationToken);
            await categoryRepository.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("Category deactivated successfully: {CategoryId} - {CategoryName}", 
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
            logger.LogError(ex, "Error deactivating category: {CategoryId}", request.Id);
            throw;
        }
    }
}