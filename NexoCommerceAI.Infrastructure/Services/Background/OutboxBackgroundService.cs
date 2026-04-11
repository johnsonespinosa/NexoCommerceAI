using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Infrastructure.Outbox;

namespace NexoCommerceAI.Infrastructure.Services.Background;

public class OutboxBackgroundService(IServiceProvider serviceProvider, ILogger<OutboxBackgroundService> logger)
    : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Outbox Background Service is starting");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();
                await processor.ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing outbox messages");
            }
            
            await Task.Delay(_interval, stoppingToken);
        }
        
        logger.LogInformation("Outbox Background Service is stopping");
    }
}