// Application/Features/Orders/Commands/ProcessPayment/ProcessPaymentCommand.cs

using MediatR;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Features.Orders.Models;

namespace NexoCommerceAI.Application.Features.Orders.Commands;

[InvalidateCache("order_by_id", "order_by_number")]
public record ProcessPaymentCommand(
    Guid OrderId,
    string PaymentMethodId,
    string CardNumber,
    string ExpiryMonth,
    string ExpiryYear,
    string Cvv,
    string CardHolderName) : IRequest<PaymentResultResponse>;