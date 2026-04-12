using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Common.Models;
using NexoCommerceAI.Application.Features.Orders.Models;

namespace NexoCommerceAI.Application.Features.Orders.Queries;

[Cacheable("orders_by_user")]
public record GetOrdersByUserQuery(Guid UserId, int PageNumber, int PageSize) 
    : IRequest<PaginatedResult<OrderResponse>>;