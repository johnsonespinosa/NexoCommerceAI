using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Domain.Events;

namespace NexoCommerceAI.Application.Features.Products.Events.Handlers;

public class OutOfStockEventHandler(ILogger<OutOfStockEventHandler> logger) : INotificationHandler<OutOfStockEvent>
{
    public Task Handle(OutOfStockEvent notification, CancellationToken cancellationToken)
    {
        logger.LogWarning(
            "🚨 OUT OF STOCK: Product '{ProductName}' (ID: {ProductId}) is now out of stock at {OccurredOn}",
            notification.ProductName,
            notification.ProductId,
            notification.OccurredOn); 
        
        // Aquí puedes agregar:
        // - Notificar al equipo de compras
        // - Actualizar estado del producto en el frontend
        // - Registrar en un sistema de analytics
        
        return Task.CompletedTask;
    }
}