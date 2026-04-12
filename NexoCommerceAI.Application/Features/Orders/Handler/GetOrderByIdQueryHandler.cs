using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Orders.Models;
using NexoCommerceAI.Application.Features.Orders.Queries;

namespace NexoCommerceAI.Application.Features.Orders.Handler;

public class GetOrderByIdQueryHandler(
    IOrderRepository orderRepository,
    ILogger<GetOrderByIdQueryHandler> logger)
    : IRequestHandler<GetOrderByIdQuery, OrderResponse?>
{
    public async Task<OrderResponse?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogDebug("Getting order {OrderId} for user {UserId}", request.OrderId, request.UserId);
        
        var order = await orderRepository.GetByIdWithItemsAsync(request.OrderId, cancellationToken);
        
        if (order == null || order.UserId != request.UserId)
            return null;
        
        return OrderResponse.MapToOrderResponse(order);
    }
}