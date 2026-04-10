using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Extensions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Commands;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Features.Products.Handlers;

public class UpdateProductStockCommandHandler(
    IProductRepository productRepository,
    ILogger<UpdateProductStockCommandHandler> logger)
    : IRequestHandler<UpdateProductStockCommand, bool>
{
    public async Task<bool> Handle(UpdateProductStockCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.UpdateStockStarted(request.ProductId, request.NewStock);
            
            var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);
            if (product == null)
            {
                logger.LogWarning("Product not found: {ProductId}", request.ProductId);
                throw new NotFoundException(nameof(Product), request.ProductId);
            }
            
            var oldStock = product.Stock;
            
            product.UpdateStock(request.NewStock);
            
            await productRepository.UpdateAsync(product, cancellationToken);
            await productRepository.SaveChangesAsync(cancellationToken);
            
            logger.StockUpdated(request.ProductId, oldStock, request.NewStock);
            
            return true;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating stock for product: {ProductId}", request.ProductId);
            throw;
        }
    }
}