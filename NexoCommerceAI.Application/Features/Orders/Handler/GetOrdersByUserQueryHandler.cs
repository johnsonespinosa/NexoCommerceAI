using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Common.Models;
using NexoCommerceAI.Application.Features.Orders.Models;
using NexoCommerceAI.Application.Features.Orders.Queries;

namespace NexoCommerceAI.Application.Features.Orders.Handler;

public class GetOrdersByUserQueryHandler(
    IOrderRepository orderRepository,
    ILogger<GetOrdersByUserQueryHandler> logger)
    : IRequestHandler<GetOrdersByUserQuery, PaginatedResult<OrderResponse>>
{
    public async Task<PaginatedResult<OrderResponse>> Handle(GetOrdersByUserQuery request, CancellationToken cancellationToken)
    {
        logger.LogDebug("Getting orders for user {UserId}, Page {PageNumber}, Size {PageSize}", 
            request.UserId, request.PageNumber, request.PageSize);
        
        var orders = await orderRepository.GetByUserIdAsync(request.UserId, request.PageNumber, request.PageSize, cancellationToken);
        var totalCount = await orderRepository.GetCountByUserIdAsync(request.UserId, cancellationToken);
        
        var items = orders.Select(OrderResponse.MapToOrderResponse).ToList();
        
        return new PaginatedResult<OrderResponse>(
            items,
            totalCount,
            request.PageNumber,
            request.PageSize);
    }
}