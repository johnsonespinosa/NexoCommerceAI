using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Payments.Commands;
using NexoCommerceAI.Application.Features.Payments.Models;
using NexoCommerceAI.Domain.Entities;
using NexoCommerceAI.Domain.Enums;

namespace NexoCommerceAI.Application.Features.Payments.Handlers;

public class CreatePaymentIntentCommandHandler(
    IOrderRepository orderRepository,
    IPaymentService paymentService,
    ILogger<CreatePaymentIntentCommandHandler> logger)
    : IRequestHandler<CreatePaymentIntentCommand, PaymentIntentResponse>
{
    public async Task<PaymentIntentResponse> Handle(CreatePaymentIntentCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating payment intent for order {OrderId}", request.OrderId);
        
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            throw new NotFoundException(nameof(Order), request.OrderId);
        
        if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.PaymentProcessing)
            throw new ValidationException($"Cannot create payment intent for order with status {order.Status}");
        
        // Actualizar estado a PaymentProcessing
        order.UpdateStatus(OrderStatus.PaymentProcessing);
        await orderRepository.UpdateAsync(order, cancellationToken);
        await orderRepository.SaveChangesAsync(cancellationToken);
        
        var paymentRequest = new PaymentRequest
        {
            OrderId = order.Id,
            Amount = order.TotalAmount.Amount,
            Currency = order.TotalAmount.Currency,
            PaymentMethodId = request.PaymentMethodId,
            CustomerEmail = order.User.Email
        };
        
        var paymentResult = await paymentService.ProcessPaymentAsync(paymentRequest, cancellationToken);
        
        if (!paymentResult.Success)
        {
            order.UpdateStatus(OrderStatus.Cancelled, paymentResult.ErrorMessage);
            await orderRepository.UpdateAsync(order, cancellationToken);
            await orderRepository.SaveChangesAsync(cancellationToken);
            throw new ValidationException(paymentResult.ErrorMessage ?? "Payment processing failed");
        }
        
        return new PaymentIntentResponse
        {
            PaymentIntentId = paymentResult.TransactionId,
            ClientSecret = paymentResult.ClientSecret ?? string.Empty,
            Amount = order.TotalAmount.Amount,
            Currency = order.TotalAmount.Currency,
            Status = "requires_confirmation"
        };
    }
}