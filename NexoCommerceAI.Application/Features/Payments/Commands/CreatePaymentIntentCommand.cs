// Application/Features/Payments/Commands/CreatePaymentIntent/CreatePaymentIntentCommand.cs

using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Features.Payments.Models;

namespace NexoCommerceAI.Application.Features.Payments.Commands;

[InvalidateCache("order_by_id", "order_by_number")]
public record CreatePaymentIntentCommand(
    Guid OrderId,
    string? PaymentMethodId = null) : IRequest<PaymentIntentResponse>;