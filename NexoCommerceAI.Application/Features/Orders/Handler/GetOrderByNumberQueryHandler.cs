using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Orders.Models;
using NexoCommerceAI.Application.Features.Orders.Queries;

namespace NexoCommerceAI.Application.Features.Orders.Handler;

public class GetOrderByNumberQueryHandler(
    IOrderRepository orderRepository,
    ILogger<GetOrderByNumberQueryHandler> logger)
    : IRequestHandler<GetOrderByNumberQuery, OrderResponse?>
{
    public async Task<OrderResponse?> Handle(GetOrderByNumberQuery request, CancellationToken cancellationToken)
    {
        logger.LogDebug("Getting order by number {OrderNumber} for user {UserId}", request.OrderNumber, request.UserId);
        
        var order = await orderRepository.GetByOrderNumberAsync(request.OrderNumber, cancellationToken);
        
        if (order == null || order.UserId != request.UserId)
            return null;
        
        return OrderResponse.MapToOrderResponse(order);
    }
}