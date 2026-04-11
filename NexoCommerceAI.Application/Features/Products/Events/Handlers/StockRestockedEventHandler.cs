using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Domain.Events;

namespace NexoCommerceAI.Application.Features.Products.Events.Handlers;

public class StockRestockedEventHandler(ILogger<StockRestockedEventHandler> logger) : INotificationHandler<StockRestockedEvent>
{
    public Task Handle(StockRestockedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "📦 STOCK RESTOCKED: Product '{ProductName}' (ID: {ProductId}) increased from {PreviousStock} to {NewStock}. Added: {QuantityAdded} units. Occurred at {OccurredOn}",
            notification.ProductName,
            notification.ProductId,
            notification.PreviousStock,
            notification.NewStock,
            notification.QuantityAdded,
            notification.OccurredOn);
        
        return Task.CompletedTask;
    }
}