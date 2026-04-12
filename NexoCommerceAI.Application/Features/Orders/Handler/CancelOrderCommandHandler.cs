using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Orders.Commands;
using NexoCommerceAI.Domain.Entities;
using NexoCommerceAI.Domain.Enums;

namespace NexoCommerceAI.Application.Features.Orders.Handler;

public class CancelOrderCommandHandler(
    IOrderRepository orderRepository,
    IInventoryService inventoryService,
    ILogger<CancelOrderCommandHandler> logger)
    : IRequestHandler<CancelOrderCommand, bool>
{
    public async Task<bool> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Cancelling order {OrderId}", request.OrderId);
        
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            throw new NotFoundException(nameof(Order), request.OrderId);
        
        if (order.Status == OrderStatus.Delivered)
            throw new ValidationException("Cannot cancel delivered order");
        
        if (order.Status == OrderStatus.Cancelled)
            throw new ValidationException("Order is already cancelled");
        
        // Liberar stock reservado
        await inventoryService.ReleaseStockAsync(request.OrderId, cancellationToken);
        
        order.Cancel(request.Reason);
        
        await orderRepository.UpdateAsync(order, cancellationToken);
        await orderRepository.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Order {OrderId} cancelled successfully", request.OrderId);
        
        return true;
    }
}