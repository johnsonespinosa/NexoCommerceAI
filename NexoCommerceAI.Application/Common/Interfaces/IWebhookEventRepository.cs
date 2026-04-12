namespace NexoCommerceAI.Application.Common.Interfaces;

public interface IWebhookEventRepository
{
    Task<bool> HasBeenProcessedAsync(string eventId, CancellationToken cancellationToken = default);
    Task MarkAsProcessedAsync(string eventId, string eventType, string? orderId = null, CancellationToken cancellationToken = default);
}