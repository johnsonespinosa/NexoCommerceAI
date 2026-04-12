using MediatR;
using NexoCommerceAI.Application.Common.Attributes;

namespace NexoCommerceAI.Application.Features.Orders.Commands;

[InvalidateCache("order_by_id", "order_by_number", "orders_by_user")]
public record UpdateOrderStatusCommand(
    Guid OrderId,
    string NewStatus,
    string? Comment = null) : IRequest<bool>;