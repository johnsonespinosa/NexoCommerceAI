// Infrastructure/Services/MockInventoryService.cs

using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Infrastructure.Services.EventBus;

public class MockInventoryService(ILogger<MockInventoryService> logger) : IInventoryService
{
    private readonly Dictionary<Guid, int> _stockSimulation = new();

    public Task<InventoryReservationResult> ReserveStockAsync(Guid orderId, IReadOnlyList<OrderItem> items, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Reserving stock for order {OrderId}", orderId);
        
        var errors = (from item in items let available = GetAvailableStock(item.ProductId) where available < item.Quantity select new ReservedItemError(item.ProductId, item.ProductName, item.Quantity, available)).ToList();

        if (errors.Count != 0)
        {
            return Task.FromResult(new InventoryReservationResult
            {
                Success = false,
                ErrorMessage = "Insufficient stock for some items",
                Errors = errors
            });
        }
        
        // Reservar stock
        foreach (var item in items)
        {
            ReserveStock(item.ProductId, item.Quantity);
        }
        
        logger.LogInformation("Stock reserved successfully for order {OrderId}", orderId);
        
        return Task.FromResult(new InventoryReservationResult { Success = true });
    }
    
    public Task<bool> ReleaseStockAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Releasing stock for order {OrderId}", orderId);
        // Implementar liberación de stock
        return Task.FromResult(true);
    }
    
    public Task<bool> ConfirmStockAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Confirming stock for order {OrderId}", orderId);
        // Implementar confirmación de stock
        return Task.FromResult(true);
    }
    
    private int GetAvailableStock(Guid productId)
    {
        // Simular stock disponible
        return _stockSimulation.GetValueOrDefault(productId, 100);
    }
    
    private void ReserveStock(Guid productId, int quantity)
    {
        if (_stockSimulation.ContainsKey(productId))
            _stockSimulation[productId] -= quantity;
        else
            _stockSimulation[productId] = 100 - quantity;
    }
}