using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Features.Orders.Models;

namespace NexoCommerceAI.Application.Features.Orders.Queries;

[Cacheable("order_by_number")]
public record GetOrderByNumberQuery(string OrderNumber, Guid UserId) : IRequest<OrderResponse?>;