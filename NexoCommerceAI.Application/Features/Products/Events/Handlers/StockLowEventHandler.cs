using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Domain.Events;

namespace NexoCommerceAI.Application.Features.Products.Events.Handlers;

public class StockLowEventHandler(ILogger<StockLowEventHandler> logger) : INotificationHandler<StockLowEvent>
{
    public Task Handle(StockLowEvent notification, CancellationToken cancellationToken)
    {
        logger.LogWarning(
            "⚠️ STOCK LOW ALERT: Product '{ProductName}' (ID: {ProductId}) has only {CurrentStock} units remaining. Threshold: {Threshold}. Occurred at {OccurredOn}",
            notification.ProductName,
            notification.ProductId,
            notification.CurrentStock,
            notification.Threshold,
            notification.OccurredOn);
        
        return Task.CompletedTask;
    }
}