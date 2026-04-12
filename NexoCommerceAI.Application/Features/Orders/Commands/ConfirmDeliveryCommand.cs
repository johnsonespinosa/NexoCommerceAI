using MediatR;
using NexoCommerceAI.Application.Common.Attributes;

namespace NexoCommerceAI.Application.Features.Orders.Commands;

[InvalidateCache("order_by_id", "order_by_number")]
public record ConfirmDeliveryCommand(Guid OrderId) : IRequest<bool>;