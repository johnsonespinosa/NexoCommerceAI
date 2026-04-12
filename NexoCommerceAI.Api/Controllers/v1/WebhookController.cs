using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Payments.Commands;
using NexoCommerceAI.Domain.Enums;
using Stripe;

namespace NexoCommerceAI.Api.Controllers.v1;

[ApiController]
[Route("api/webhooks")]
[AllowAnonymous]
[Produces("application/json")]
public class WebhookController(
    IMediator mediator,
    IOrderRepository orderRepository,
    ILogger<WebhookController> logger)
    : ControllerBase
{
    /// <summary>
    /// Endpoint para webhooks de Stripe
    /// </summary>
    [HttpPost("stripe")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> HandleStripeWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        
        try
        {
            // Verificar firma del webhook (en producción usar Stripe.WebhookSignature)
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                "whsec_test_webhook_secret"
            );

            logger.LogInformation("Received Stripe webhook: {Type}", stripeEvent.Type);

            switch (stripeEvent.Type)
            {
                case "payment_intent.succeeded":
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    await HandlePaymentIntentSucceeded(paymentIntent);
                    break;
                    
                case "payment_intent.payment_failed":
                    var failedPaymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    await HandlePaymentIntentFailed(failedPaymentIntent);
                    break;
                    
                case "charge.refunded":
                    var refund = stripeEvent.Data.Object as Charge;
                    await HandleChargeRefunded(refund);
                    break;
                    
                default:
                    logger.LogDebug("Unhandled webhook type: {Type}", stripeEvent.Type);
                    break;
            }
            
            return Ok();
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe webhook error");
            return BadRequest();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing webhook");
            return BadRequest();
        }
    }
    
    private async Task HandlePaymentIntentSucceeded(PaymentIntent? paymentIntent)
    {
        if (paymentIntent?.Metadata?.TryGetValue("OrderId", out var orderIdStr) == true &&
            Guid.TryParse(orderIdStr, out var orderId))
        {
            logger.LogInformation("Payment succeeded for order {OrderId}", orderId);
            
            var result = await mediator.Send(new ConfirmPaymentCommand(orderId, paymentIntent.Id));
            
            if (result.Success)
            {
                logger.LogInformation("Order {OrderNumber} confirmed after payment", result.OrderNumber);
            }
        }
    }
    
    private async Task HandlePaymentIntentFailed(PaymentIntent? paymentIntent)
    {
        if (paymentIntent?.Metadata?.TryGetValue("OrderId", out var orderIdStr) == true &&
            Guid.TryParse(orderIdStr, out var orderId))
        {
            logger.LogWarning("Payment failed for order {OrderId}: {Error}", 
                orderId, paymentIntent.LastPaymentError?.Message);
            
            var order = await orderRepository.GetByIdAsync(orderId);
            if (order != null && order.Status == OrderStatus.PaymentProcessing)
            {
                order.UpdateStatus(OrderStatus.Cancelled, paymentIntent.LastPaymentError?.Message);
                await orderRepository.UpdateAsync(order);
                await orderRepository.SaveChangesAsync();
            }
        }
    }
    
    private async Task HandleChargeRefunded(Charge? charge)
    {
        if (charge?.Metadata?.TryGetValue("OrderId", out var orderIdStr) == true &&
            Guid.TryParse(orderIdStr, out var orderId))
        {
            logger.LogInformation("Charge refunded for order {OrderId}", orderId);
            
            var order = await orderRepository.GetByIdAsync(orderId);
            if (order != null)
            {
                order.UpdateStatus(OrderStatus.Refunded, "Payment refunded");
                await orderRepository.UpdateAsync(order);
                await orderRepository.SaveChangesAsync();
            }
        }
    }
}