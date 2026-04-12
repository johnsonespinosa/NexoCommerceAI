// Infrastructure/Services/Background/OrderExpirationService.cs

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Common.Settings;

namespace NexoCommerceAI.Infrastructure.Services;

public class OrderExpirationService(
    IServiceProvider serviceProvider,
    ILogger<OrderExpirationService> logger,
    IOptions<OrderExpirationSettings> settings)
    : BackgroundService
{
    private readonly OrderExpirationSettings _settings = settings.Value;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Order Expiration Service is starting");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExpirePendingOrdersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error expiring orders");
            }
            
            await Task.Delay(_interval, stoppingToken);
        }
        
        logger.LogInformation("Order Expiration Service is stopping");
    }

    private async Task ExpirePendingOrdersAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        var inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        
        var expirationDate = DateTime.UtcNow.AddMinutes(-_settings.PendingOrderExpirationMinutes);
        var pendingOrders = await orderRepository.GetPendingOrdersAsync(expirationDate, cancellationToken);
        
        foreach (var order in pendingOrders)
        {
            logger.LogInformation("Expiring order {OrderNumber} created at {CreatedAt}", 
                order.OrderNumber, order.CreatedAt);
            
            order.Cancel($"Order expired after {_settings.PendingOrderExpirationMinutes} minutes without payment");
            await inventoryService.ReleaseStockAsync(order.Id, cancellationToken);
            await orderRepository.UpdateAsync(order, cancellationToken);
        }
        
        if (pendingOrders.Any())
        {
            await orderRepository.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Expired {Count} pending orders", pendingOrders.Count);
        }
    }
}