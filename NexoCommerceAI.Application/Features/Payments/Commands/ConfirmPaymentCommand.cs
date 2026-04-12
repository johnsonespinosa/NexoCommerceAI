using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Features.Payments.Models;

namespace NexoCommerceAI.Application.Features.Payments.Commands;

[InvalidateCache("order_by_id", "order_by_number", "orders_by_user")]
public record ConfirmPaymentCommand(
    Guid OrderId,
    string PaymentIntentId) : IRequest<ConfirmPaymentResponse>;