using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Orders.Commands;
using NexoCommerceAI.Domain.Entities;
using NexoCommerceAI.Domain.Enums;

namespace NexoCommerceAI.Application.Features.Orders.Handler;

public class UpdateOrderStatusCommandHandler(
    IOrderRepository orderRepository,
    ILogger<UpdateOrderStatusCommandHandler> logger)
    : IRequestHandler<UpdateOrderStatusCommand, bool>
{
    public async Task<bool> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating order {OrderId} status to {NewStatus}", request.OrderId, request.NewStatus);
        
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            throw new NotFoundException(nameof(Order), request.OrderId);
        
        var newStatus = Enum.Parse<OrderStatus>(request.NewStatus, true);
        order.UpdateStatus(newStatus, request.Comment, "Admin");
        
        await orderRepository.UpdateAsync(order, cancellationToken);
        await orderRepository.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Order {OrderId} status updated to {NewStatus}", request.OrderId, request.NewStatus);
        
        return true;
    }
}