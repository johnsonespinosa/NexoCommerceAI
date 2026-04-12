using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Domain.Entities;
using NexoCommerceAI.Domain.Enums;
using NexoCommerceAI.Domain.Events;
using Stripe;

namespace NexoCommerceAI.Infrastructure.Services;

public class WebhookProcessingService(
    IServiceProvider serviceProvider,
    ILogger<WebhookProcessingService> logger,
    IStripeWebhookService webhookService)
{
    private readonly int[] _retryDelays = { 1000, 2000, 5000, 10000, 30000 }; // 1s, 2s, 5s, 10s, 30s

    public async Task ProcessWebhookWithRetryAsync(string json, string signature, CancellationToken cancellationToken = default)
    {
        var retryCount = 0;
        Exception? lastException = null;
        
        while (retryCount <= _retryDelays.Length)
        {
            try
            {
                await ProcessWebhookAsync(json, signature, cancellationToken);
                return; // Éxito, salir del bucle
            }
            catch (Exception ex) when (retryCount < _retryDelays.Length)
            {
                lastException = ex;
                var delay = _retryDelays[retryCount];
                logger.LogWarning(ex, "Webhook processing failed, retrying in {Delay}ms (attempt {RetryCount}/{MaxRetries})", 
                    delay, retryCount + 1, _retryDelays.Length);
                
                await Task.Delay(delay, cancellationToken);
                retryCount++;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Webhook processing failed permanently after {RetryCount} attempts", retryCount);
                throw;
            }
        }
        
        throw lastException ?? new Exception("Webhook processing failed after all retries");
    }

    private async Task ProcessWebhookAsync(string json, string signature, CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var webhookEventRepo = scope.ServiceProvider.GetRequiredService<IWebhookEventRepository>();
        var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        var paymentHistoryRepo = scope.ServiceProvider.GetRequiredService<IPaymentHistoryRepository>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        // 1. Validar firma
        var stripeEvent = await webhookService.ConstructEventAsync(json, signature, cancellationToken);
        
        // 2. Verificar idempotencia
        if (await webhookEventRepo.HasBeenProcessedAsync(stripeEvent.Id, cancellationToken))
        {
            logger.LogInformation("Webhook event {EventId} already processed, skipping", stripeEvent.Id);
            return;
        }
        
        logger.LogInformation("Processing webhook event {EventId} of type {EventType}", stripeEvent.Id, stripeEvent.Type);
        
        // 3. Procesar según tipo de evento
        string? orderId = null;
        
        switch (stripeEvent.Type)
        {
            case "payment_intent.created":
                orderId = await HandlePaymentIntentCreated(stripeEvent, orderRepository, paymentHistoryRepo);
                break;
                
            case "payment_intent.succeeded":
                orderId = await HandlePaymentIntentSucceeded(stripeEvent, orderRepository, paymentHistoryRepo, mediator);
                break;
                
            case "payment_intent.payment_failed":
                orderId = await HandlePaymentIntentFailed(stripeEvent, orderRepository, paymentHistoryRepo);
                break;
                
            case "charge.refunded":
                orderId = await HandleChargeRefunded(stripeEvent, orderRepository, paymentHistoryRepo);
                break;
        }
        
        // 4. Marcar como procesado
        await webhookEventRepo.MarkAsProcessedAsync(stripeEvent.Id, stripeEvent.Type, orderId, cancellationToken);
        
        logger.LogInformation("Webhook event {EventId} processed successfully", stripeEvent.Id);
    }
    
    private async Task<string?> HandlePaymentIntentSucceeded(
        Event stripeEvent,
        IOrderRepository orderRepository,
        IPaymentHistoryRepository paymentHistoryRepo,
        IMediator mediator)
    {
        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
        if (paymentIntent?.Metadata?.TryGetValue("OrderId", out var orderIdStr) != true)
            return null;
        
        var orderId = Guid.Parse(orderIdStr);
        var order = await orderRepository.GetByIdAsync(orderId);
        
        if (order == null || order.Status != OrderStatus.PaymentProcessing)
            return null;
        
        // Actualizar orden
        order.UpdateStatus(OrderStatus.Paid);
        await orderRepository.UpdateAsync(order);
        await orderRepository.SaveChangesAsync();
        
        // Registrar historial de pago
        var paymentHistory = PaymentHistory.CreatePaymentSucceeded(
            orderId, 
            paymentIntent.Id, 
            (decimal)paymentIntent.Amount / 100,
            paymentIntent.Metadata?.ToString() ?? string.Empty);
        await paymentHistoryRepo.AddAsync(paymentHistory);
        
        // Publicar eventos
        await mediator.Publish(new OrderConfirmedIntegrationEvent(order.Id, order.OrderNumber, order.UserId, order.TotalAmount.Amount, DateTime.UtcNow));
        await mediator.Publish(new PaymentProcessedIntegrationEvent(order.Id, order.OrderNumber, paymentIntent.Id, order.TotalAmount.Amount, true, DateTime.UtcNow));
        
        return orderId.ToString();
    }
    
    private async Task<string?> HandlePaymentIntentCreated(
        Event stripeEvent,
        IOrderRepository orderRepository,
        IPaymentHistoryRepository paymentHistoryRepo)
    {
        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
        if (paymentIntent?.Metadata?.TryGetValue("OrderId", out var orderIdStr) != true)
            return null;
        
        var paymentHistory = PaymentHistory.CreatePaymentIntentCreated(
            Guid.Parse(orderIdStr), 
            paymentIntent.Id, 
            (decimal)paymentIntent.Amount / 100);
        await paymentHistoryRepo.AddAsync(paymentHistory);
        
        return orderIdStr;
    }
    
    private async Task<string?> HandlePaymentIntentFailed(
        Event stripeEvent,
        IOrderRepository orderRepository,
        IPaymentHistoryRepository paymentHistoryRepo)
    {
        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
        if (paymentIntent?.Metadata?.TryGetValue("OrderId", out var orderIdStr) != true)
            return null;
        
        var orderId = Guid.Parse(orderIdStr);
        var order = await orderRepository.GetByIdAsync(orderId);
        
        if (order != null && order.Status == OrderStatus.PaymentProcessing)
        {
            order.UpdateStatus(OrderStatus.Cancelled, paymentIntent.LastPaymentError?.Message);
            await orderRepository.UpdateAsync(order);
            await orderRepository.SaveChangesAsync();
        }
        
        var paymentHistory = PaymentHistory.CreatePaymentFailed(
            orderId, 
            paymentIntent.Id, 
            (decimal)paymentIntent.Amount / 100,
            paymentIntent.LastPaymentError?.Message ?? "Unknown error");
        await paymentHistoryRepo.AddAsync(paymentHistory);
        
        return orderId.ToString();
    }
    
    private async Task<string?> HandleChargeRefunded(
        Event stripeEvent,
        IOrderRepository orderRepository,
        IPaymentHistoryRepository paymentHistoryRepo)
    {
        var charge = stripeEvent.Data.Object as Charge;
        var paymentIntentId = charge?.PaymentIntentId;
        
        if (string.IsNullOrEmpty(paymentIntentId))
            return null;
        
        // Buscar orden por PaymentIntentId
        var order = await orderRepository.GetByPaymentIntentIdAsync(paymentIntentId);
        if (order == null)
            return null;
        
        order.UpdateStatus(OrderStatus.Refunded, "Payment refunded");
        await orderRepository.UpdateAsync(order);
        await orderRepository.SaveChangesAsync();
        
        var paymentHistory = PaymentHistory.CreateRefunded(order.Id, paymentIntentId, (decimal)(charge?.Amount ?? 0) / 100);
        await paymentHistoryRepo.AddAsync(paymentHistory);
        
        return order.Id.ToString();
    }
}