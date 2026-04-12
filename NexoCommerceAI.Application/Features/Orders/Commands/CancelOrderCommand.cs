using MediatR;
using NexoCommerceAI.Application.Common.Attributes;

namespace NexoCommerceAI.Application.Features.Orders.Commands;

[InvalidateCache("order_by_id", "order_by_number", "orders_by_user")]
public record CancelOrderCommand(Guid OrderId, string? Reason = null) : IRequest<bool>;