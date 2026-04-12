using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Orders.Commands;
using NexoCommerceAI.Domain.Entities;
using NexoCommerceAI.Domain.Enums;

namespace NexoCommerceAI.Application.Features.Orders.Handler;

public class ConfirmDeliveryCommandHandler(
    IOrderRepository orderRepository,
    ILogger<ConfirmDeliveryCommandHandler> logger)
    : IRequestHandler<ConfirmDeliveryCommand, bool>
{
    public async Task<bool> Handle(ConfirmDeliveryCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Confirming delivery for order {OrderId}", request.OrderId);
        
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            throw new NotFoundException(nameof(Order), request.OrderId);
        
        if (order.Status != OrderStatus.Shipped)
            throw new ValidationException($"Cannot confirm delivery for order with status {order.Status}");
        
        order.UpdateStatus(OrderStatus.Delivered);
        
        await orderRepository.UpdateAsync(order, cancellationToken);
        await orderRepository.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Delivery confirmed for order {OrderId}", request.OrderId);
        
        return true;
    }
}