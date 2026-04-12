using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Features.Orders.Models;

namespace NexoCommerceAI.Application.Features.Orders.Commands;

[InvalidateCache("orders", "order_summary")]
public record CreateOrderCommand(
    Guid UserId,
    AddressDto ShippingAddress,
    AddressDto BillingAddress,
    string? CustomerNotes = null) : IRequest<OrderResponse>;