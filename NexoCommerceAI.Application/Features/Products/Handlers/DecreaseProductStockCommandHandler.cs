using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Extensions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Commands;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Products.Handlers;

public class DecreaseProductStockCommandHandler(
    IProductRepository productRepository,
    ILogger<DecreaseProductStockCommandHandler> logger)
    : IRequestHandler<DecreaseProductStockCommand, bool>
{
    public async Task<bool> Handle(DecreaseProductStockCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.DecreaseStockStarted(request.ProductId, request.Quantity);
            
            var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product == null)
            {
                logger.LogWarning("Product not found: {ProductId}", request.ProductId);
                throw new NotFoundException(nameof(Product), request.ProductId);
            }
            
            var oldStock = product.Stock;
            
            // Verificar si el producto está activo
            if (!product.IsActive)
            {
                logger.CannotDecreaseStockInactive(product.Id, product.Name);
                throw new ValidationException($"Cannot decrease stock for inactive product '{product.Name}'");
            }
            
            // Verificar si el producto está eliminado
            if (product.IsDeleted)
            {
                logger.CannotDecreaseStockDeleted(product.Id, product.Name);
                throw new ValidationException($"Cannot decrease stock for deleted product '{product.Name}'");
            }
            
            // Intentar disminuir el stock
            try
            {
                product.DecreaseStock(request.Quantity);
            }
            catch (InvalidOperationException ex)
            {
                logger.InsufficientStock(product.Id, oldStock, request.Quantity);
                throw new ValidationException($"Insufficient stock for product '{product.Name}'. Available: {oldStock}, Requested: {request.Quantity}");
            }
            
            await productRepository.UpdateAsync(product, cancellationToken);
            await productRepository.SaveChangesAsync(cancellationToken);
            
            logger.StockDecreased(request.ProductId, oldStock, product.Stock);
            
            // Verificar si el producto ahora tiene stock bajo
            if (product.IsLowStock())
            {
                logger.LowStockWarning(product.Id, product.Name, product.Stock);
            }
            
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
            logger.LogError(ex, "Error decreasing stock for product: {ProductId}", request.ProductId);
            throw;
        }
    }
}