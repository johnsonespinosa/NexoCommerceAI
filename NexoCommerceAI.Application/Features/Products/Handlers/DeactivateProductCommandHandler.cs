using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Extensions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Commands;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Products.Handlers;

public class DeactivateProductCommandHandler(
    IProductRepository productRepository,
    ILogger<DeactivateProductCommandHandler> logger)
    : IRequestHandler<DeactivateProductCommand, bool>
{
    public async Task<bool> Handle(DeactivateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.DeactivateProductStarted(request.ProductId);
            
            var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product == null)
            {
                logger.LogWarning("Product not found: {ProductId}", request.ProductId);
                throw new NotFoundException(nameof(Product), request.ProductId);
            }
            
            if (product.IsDeleted)
            {
                logger.CannotDeactivateDeletedProduct(product.Id, product.Name);
                throw new ValidationException($"Cannot deactivate deleted product '{product.Name}'");
            }
            
            if (!product.IsActive)
            {
                logger.ProductAlreadyInactive(product.Id, product.Name);
                return false;
            }
            
            product.Deactivate();
            
            await productRepository.UpdateAsync(product, cancellationToken);
            await productRepository.SaveChangesAsync(cancellationToken);
            
            logger.ProductDeactivated(product.Id, product.Name);
            
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
            logger.LogError(ex, "Error deactivating product: {ProductId}", request.ProductId);
            throw;
        }
    }
}