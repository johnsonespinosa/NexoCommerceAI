using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Extensions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Commands;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Products.Handlers;

public class ActivateProductCommandHandler(
    IProductRepository productRepository,
    ILogger<ActivateProductCommandHandler> logger)
    : IRequestHandler<ActivateProductCommand, bool>
{
    public async Task<bool> Handle(ActivateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.ActivateProductStarted(request.ProductId);
            
            var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product == null)
            {
                logger.LogWarning("Product not found: {ProductId}", request.ProductId);
                throw new NotFoundException(nameof(Product), request.ProductId);
            }
            
            if (product.IsDeleted)
            {
                logger.CannotActivateDeletedProduct(product.Id, product.Name);
                throw new ValidationException($"Cannot activate deleted product '{product.Name}'");
            }
            
            if (product.IsActive)
            {
                logger.ProductAlreadyActive(product.Id, product.Name);
                return false;
            }
            
            product.Activate();
            
            await productRepository.UpdateAsync(product, cancellationToken);
            await productRepository.SaveChangesAsync(cancellationToken);
            
            logger.ProductActivated(product.Id, product.Name);
            
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
            logger.LogError(ex, "Error activating product: {ProductId}", request.ProductId);
            throw;
        }
    }
}