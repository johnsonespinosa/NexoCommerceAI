using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Features.Orders.Models;

namespace NexoCommerceAI.Application.Features.Orders.Queries;

[Cacheable("order_by_id")]
public record GetOrderByIdQuery(Guid OrderId, Guid UserId) : IRequest<OrderResponse?>;