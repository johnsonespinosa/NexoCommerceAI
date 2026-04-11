using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Domain.Events;

namespace NexoCommerceAI.Application.Features.Products.Events.Handlers;

public class ProductImageRemovedEventHandler(ILogger<ProductImageRemovedEventHandler> logger) 
    : INotificationHandler<ProductImageRemovedEvent>
{
    public Task Handle(ProductImageRemovedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "🗑️ IMAGE REMOVED: Image '{ImageId}' removed from product '{ProductName}' (ID: {ProductId}). URL: {ImageUrl}. PublicId: {PublicId}. Occurred at {OccurredOn}",
            notification.ImageId,
            notification.ProductName,
            notification.ProductId,
            notification.ImageUrl,
            notification.PublicId,
            notification.OccurredOn);
        
        // Aquí podrías agregar lógica adicional como:
        // - Eliminar la imagen del CDN (Cloudinary/S3)
        // - Actualizar cachés
        // - Notificar a sistemas externos
        
        return Task.CompletedTask;
    }
}