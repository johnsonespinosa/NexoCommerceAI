using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Commands;
using NexoCommerceAI.Application.Features.Products.Models;
using NexoCommerceAI.Application.Features.Products.Specifications;

namespace NexoCommerceAI.Application.Features.Products.Handlers;

public class BulkUpdateStockCommandHandler(
    IProductRepository productRepository,
    ILogger<BulkUpdateStockCommandHandler> logger)
    : IRequestHandler<BulkUpdateStockCommand, BulkUpdateStockResult>
{
    public async Task<BulkUpdateStockResult> Handle(BulkUpdateStockCommand request, CancellationToken cancellationToken)
    {
        var result = new BulkUpdateStockResult
        {
            TotalItems = request.Items.Count
        };
        
        var errors = new List<BulkStockUpdateError>();
        var successCount = 0;
        
        logger.LogInformation("Starting bulk stock update for {Count} products", request.Items.Count);
        
        // Obtener todos los productos en una sola consulta usando Specification
        var productIds = request.Items.Select(x => x.ProductId).Distinct().ToList();
        var spec = new ProductsByIdsSpec(productIds);
        var productsList = await productRepository.ListAsync(spec, cancellationToken);
        var products = productsList.ToDictionary(p => p.Id);
        
        foreach (var item in request.Items)
        {
            try
            {
                if (!products.TryGetValue(item.ProductId, out var product))
                {
                    errors.Add(new BulkStockUpdateError
                    {
                        ProductId = item.ProductId,
                        ErrorMessage = "Product not found or deleted",
                        AttemptedStock = item.NewStock
                    });
                    continue;
                }
                
                if (!product.IsActive)
                {
                    errors.Add(new BulkStockUpdateError
                    {
                        ProductId = item.ProductId,
                        ErrorMessage = $"Product '{product.Name}' is inactive",
                        AttemptedStock = item.NewStock
                    });
                    continue;
                }
                
                var oldStock = product.Stock;
                product.UpdateStock(item.NewStock);
                successCount++;
                
                await productRepository.UpdateAsync(product, cancellationToken);
                
                logger.LogDebug("Updated stock for product {ProductId} - {ProductName}: {OldStock} → {NewStock}", 
                    product.Id, product.Name, oldStock, item.NewStock);
                
                // Verificar stock bajo
                if (product.IsLowStock())
                {
                    logger.LogWarning("Product {ProductId} - {ProductName} is now low on stock: {Stock} units remaining", 
                        product.Id, product.Name, product.Stock);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating stock for product {ProductId}", item.ProductId);
                errors.Add(new BulkStockUpdateError
                {
                    ProductId = item.ProductId,
                    ErrorMessage = ex.Message,
                    AttemptedStock = item.NewStock
                });
            }
        }
        
        // Guardar cambios
        if (successCount > 0)
        {
            await productRepository.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Bulk stock update completed: {SuccessCount} succeeded, {FailureCount} failed", 
                successCount, errors.Count);
        }
        
        result.SuccessCount = successCount;
        result.FailureCount = errors.Count;
        result.Errors = errors;
        
        return result;
    }
}