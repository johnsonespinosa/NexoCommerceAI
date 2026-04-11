using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Domain.Events;

namespace NexoCommerceAI.Application.Features.Products.Events.Handlers;

public class ProductImageAddedEventHandler(ILogger<ProductImageAddedEventHandler> logger) 
    : INotificationHandler<ProductImageAddedEvent>
{
    public Task Handle(ProductImageAddedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "🖼️ IMAGE ADDED: Image '{ImageId}' added to product '{ProductName}' (ID: {ProductId}). URL: {ImageUrl}. IsMain: {IsMain}. Occurred at {OccurredOn}",
            notification.ImageId,
            notification.ProductName,
            notification.ProductId,
            notification.ImageUrl,
            notification.IsMain,
            notification.OccurredOn);
        
        return Task.CompletedTask;
    }
}