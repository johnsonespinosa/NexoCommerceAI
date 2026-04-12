using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NexoCommerceAI.Api.Controllers.v1;

[ApiController]
[Route("api/webhooks")]
[AllowAnonymous]
[Produces("application/json")]
public class WebhookController(IMediator mediator, ILogger<WebhookController> logger) : ControllerBase
{
    /// <summary>
    /// Endpoint para webhooks de Stripe
    /// </summary>
    [HttpPost("stripe")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> HandleStripeWebhook([FromBody] StripeWebhookPayload payload)
    {
        logger.LogInformation("Received Stripe webhook: {Type}", payload.Type);
        
        try
        {
            switch (payload.Type)
            {
                case "payment_intent.succeeded":
                    await HandlePaymentSuccess(payload);
                    break;
                case "payment_intent.payment_failed":
                    await HandlePaymentFailure(payload);
                    break;
                default:
                    logger.LogDebug("Unhandled webhook type: {Type}", payload.Type);
                    break;
            }
            
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing webhook");
            return BadRequest();
        }
    }
    
    private async Task HandlePaymentSuccess(StripeWebhookPayload payload)
    {
        var data = payload.Data?.Object;
        if (data?.Metadata?.OrderId != null)
        {
            // Aquí se procesaría la confirmación de pago
            logger.LogInformation("Payment succeeded for order {OrderId}", data.Metadata.OrderId);
        }
    }
    
    private async Task HandlePaymentFailure(StripeWebhookPayload payload)
    {
        logger.LogWarning("Payment failed: {Error}", payload.Data?.Object?.LastPaymentError?.Message);
    }
}

public class StripeWebhookPayload
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public StripeData? Data { get; set; }
}

public class StripeData
{
    public StripeObject? Object { get; set; }
}

public class StripeObject
{
    public string Id { get; set; } = string.Empty;
    public StripeMetadata? Metadata { get; set; }
    public StripeLastPaymentError? LastPaymentError { get; set; }
}

public class StripeMetadata
{
    public string? OrderId { get; set; }
}

public class StripeLastPaymentError
{
    public string Message { get; set; } = string.Empty;
}