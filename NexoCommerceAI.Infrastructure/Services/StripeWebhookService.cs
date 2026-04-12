using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Common.Settings;
using Stripe;

namespace NexoCommerceAI.Infrastructure.Services;

public class StripeWebhookService(
    IOptions<StripeSettings> settings,
    ILogger<StripeWebhookService> logger)
    : IStripeWebhookService
{
    private readonly StripeSettings _settings = settings.Value;

    public Task<Event> ConstructEventAsync(string json, string signature, CancellationToken cancellationToken = default)
    {
        try
        {
            // Verificar firma del webhook
            // El parámetro tolerance es en segundos (long)
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                signature,
                _settings.WebhookSecret,
                tolerance: 300 // 5 minutos en segundos
            );
            
            logger.LogInformation("Webhook signature verified successfully for event {EventId}", stripeEvent.Id);
            return Task.FromResult(stripeEvent);
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe webhook signature verification failed");
            throw new UnauthorizedAccessException("Invalid webhook signature", ex);
        }
    }
}