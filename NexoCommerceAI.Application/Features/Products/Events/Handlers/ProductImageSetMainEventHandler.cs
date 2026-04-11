using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Domain.Events;

namespace NexoCommerceAI.Application.Features.Products.Events.Handlers;

public class ProductImageSetMainEventHandler(ILogger<ProductImageSetMainEventHandler> logger) 
    : INotificationHandler<ProductImageSetMainEvent>
{
    public Task Handle(ProductImageSetMainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "⭐ MAIN IMAGE CHANGED: Image '{ImageId}' is now the main image for product '{ProductName}' (ID: {ProductId}). URL: {ImageUrl}. Occurred at {OccurredOn}",
            notification.ImageId,
            notification.ProductName,
            notification.ProductId,
            notification.ImageUrl,
            notification.OccurredOn);
        
        return Task.CompletedTask;
    }
}