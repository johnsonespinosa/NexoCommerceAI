using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Orders.Commands;
using NexoCommerceAI.Application.Features.Orders.Models;
using NexoCommerceAI.Domain.Entities;
using NexoCommerceAI.Domain.Enums;

namespace NexoCommerceAI.Application.Features.Orders.Handler;

public class ProcessPaymentCommandHandler(
    IOrderRepository orderRepository,
    IPaymentService paymentService,
    ILogger<ProcessPaymentCommandHandler> logger)
    : IRequestHandler<ProcessPaymentCommand, PaymentResultResponse>
{
    public async Task<PaymentResultResponse> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing payment for order {OrderId}", request.OrderId);
        
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            throw new NotFoundException(nameof(Order), request.OrderId);
        
        if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.PaymentProcessing)
            throw new ValidationException($"Cannot process payment for order with status {order.Status}");
        
        // Actualizar estado a PaymentProcessing
        order.UpdateStatus(OrderStatus.PaymentProcessing);
        
        var paymentRequest = new PaymentRequest
        {
            Amount = order.TotalAmount.Amount,
            Currency = order.TotalAmount.Currency,
            PaymentMethodId = request.PaymentMethodId,
            CardNumber = request.CardNumber,
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            Cvv = request.Cvv,
            CardHolderName = request.CardHolderName
        };
        
        var paymentResult = await paymentService.ProcessPaymentAsync(paymentRequest, cancellationToken);
        
        if (paymentResult.Success)
        {
            order.UpdateStatus(OrderStatus.Paid);
            await orderRepository.UpdateAsync(order, cancellationToken);
            await orderRepository.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("Payment successful for order {OrderId}", request.OrderId);
            
            return new PaymentResultResponse
            {
                Success = true,
                TransactionId = paymentResult.TransactionId,
                Last4 = paymentResult.Last4,
                CardType = paymentResult.CardType
            };
        }
        
        order.UpdateStatus(OrderStatus.Cancelled, paymentResult.ErrorMessage);
        await orderRepository.UpdateAsync(order, cancellationToken);
        await orderRepository.SaveChangesAsync(cancellationToken);
        
        logger.LogWarning("Payment failed for order {OrderId}: {Error}", request.OrderId, paymentResult.ErrorMessage);
        
        return new PaymentResultResponse
        {
            Success = false,
            ErrorMessage = paymentResult.ErrorMessage
        };
    }
}