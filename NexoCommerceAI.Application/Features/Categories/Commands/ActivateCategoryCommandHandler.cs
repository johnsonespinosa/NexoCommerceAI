using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Categories.Commands;

public class ActivateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    ILogger<ActivateCategoryCommandHandler> logger)
    : IRequestHandler<ActivateCategoryCommand, bool>
{
    public async Task<bool> Handle(ActivateCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Activating category: {CategoryId}", request.Id);
            
            var category = await categoryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (category == null)
                throw new NotFoundException(nameof(Category), request.Id);
            
            if (category.IsDeleted)
            {
                logger.LogWarning("Cannot activate deleted category: {CategoryId} - {CategoryName}", 
                    category.Id, category.Name);
                throw new ValidationException($"Cannot activate deleted category '{category.Name}'");
            }
            
            if (category.IsActive)
            {
                logger.LogInformation("Category already active: {CategoryId} - {CategoryName}", 
                    category.Id, category.Name);
                return false;
            }
            
            category.Activate();
            
            await categoryRepository.UpdateAsync(category, cancellationToken);
            await categoryRepository.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("Category activated successfully: {CategoryId} - {CategoryName}", 
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
            logger.LogError(ex, "Error activating category: {CategoryId}", request.Id);
            throw;
        }
    }
}