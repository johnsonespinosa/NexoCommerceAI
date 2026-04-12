// Application/Features/Payments/Commands/ConfirmPayment/ConfirmPaymentCommandHandler.cs

using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Payments.Commands;
using NexoCommerceAI.Application.Features.Payments.Models;
using NexoCommerceAI.Domain.Entities;
using NexoCommerceAI.Domain.Enums;
using NexoCommerceAI.Domain.Events;

namespace NexoCommerceAI.Application.Features.Payments.Handlers;

public class ConfirmPaymentCommandHandler(
    IOrderRepository orderRepository,
    IPaymentService paymentService,
    IMediator mediator,
    ILogger<ConfirmPaymentCommandHandler> logger)
    : IRequestHandler<ConfirmPaymentCommand, ConfirmPaymentResponse>
{
    public async Task<ConfirmPaymentResponse> Handle(ConfirmPaymentCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Confirming payment for order {OrderId} with intent {PaymentIntentId}", 
            request.OrderId, request.PaymentIntentId);
        
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            throw new NotFoundException(nameof(Order), request.OrderId);
        
        if (order.Status != OrderStatus.PaymentProcessing)
            throw new ValidationException($"Cannot confirm payment for order with status {order.Status}");
        
        // En un escenario real, aquí verificaríamos el estado del PaymentIntent con Stripe
        // Por ahora, asumimos que el pago fue exitoso
        
        order.UpdateStatus(OrderStatus.Paid);
        await orderRepository.UpdateAsync(order, cancellationToken);
        await orderRepository.SaveChangesAsync(cancellationToken);
        
        // Publicar eventos de integración
        await mediator.Publish(new OrderConfirmedIntegrationEvent(
            order.Id,
            order.OrderNumber,
            order.UserId,
            order.TotalAmount.Amount,
            DateTime.UtcNow), cancellationToken);
        
        await mediator.Publish(new PaymentProcessedIntegrationEvent(
            order.Id,
            order.OrderNumber,
            request.PaymentIntentId,
            order.TotalAmount.Amount,
            true,
            DateTime.UtcNow), cancellationToken);
        
        logger.LogInformation("Payment confirmed for order {OrderId}", request.OrderId);
        
        return new ConfirmPaymentResponse
        {
            Success = true,
            OrderNumber = order.OrderNumber,
            Status = order.Status.ToString(),
            TransactionId = request.PaymentIntentId
        };
    }
}