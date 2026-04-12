using Stripe;

namespace NexoCommerceAI.Application.Common.Interfaces;

public interface IStripeWebhookService
{
    Task<Event> ConstructEventAsync(string json, string signature, CancellationToken cancellationToken = default);
}