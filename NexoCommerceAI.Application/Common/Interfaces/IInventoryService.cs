using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Common.Interfaces;

public interface IInventoryService
{
    Task<InventoryReservationResult> ReserveStockAsync(Guid orderId, IReadOnlyList<OrderItem> items, CancellationToken cancellationToken = default);
    Task<bool> ReleaseStockAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<bool> ConfirmStockAsync(Guid orderId, CancellationToken cancellationToken = default);
}

public class InventoryReservationResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public IReadOnlyList<ReservedItemError>? Errors { get; set; }
}

public record ReservedItemError(Guid ProductId, string ProductName, int RequestedQuantity, int AvailableQuantity);