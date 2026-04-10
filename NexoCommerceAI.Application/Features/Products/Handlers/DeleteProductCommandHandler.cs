using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Extensions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Commands;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Products.Handlers;

public class DeleteProductCommandHandler(
    IProductRepository productRepository,
    ILogger<DeleteProductCommandHandler> logger)
    : IRequestHandler<DeleteProductCommand, bool>
{
    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.ProductDeleteStarted(request.Id);
            
            // Obtener el producto
            var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);
            if (product == null)
            {
                logger.LogWarning("Product not found for deletion: {ProductId}", request.Id);
                throw new NotFoundException(nameof(Product), request.Id);
            }
            
            // Verificar si ya está eliminado
            if (product.IsDeleted)
            {
                logger.LogWarning("Product already deleted: {ProductId} - {ProductName}", 
                    product.Id, product.Name);
                return false;
            }
            
            // Log del estado actual antes de eliminar
            logger.LogDebug(
                "Product details before deletion - Name: {Name}, Sku: {Sku}, Stock: {Stock}, IsActive: {IsActive}",
                product.Name, product.Sku, product.Stock, product.IsActive);
            
            // Regla de negocio: No eliminar productos con stock positivo
            if (product.Stock > 0)
            {
                logger.ProductDeleteBlocked(product.Id, product.Stock);
                throw new ValidationException($"Cannot delete product '{product.Name}' because it has {product.Stock} units in stock. Please reduce stock to zero first.");
            }
            
            // Realizar soft delete
            product.SoftDelete();
            
            await productRepository.UpdateAsync(product, cancellationToken);
            await productRepository.SaveChangesAsync(cancellationToken);
            
            logger.ProductDeleted(product.Id, product.Name);
            
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
            logger.LogError(ex, "Unexpected error deleting product: {ProductId}", request.Id);
            throw;
        }
    }
}