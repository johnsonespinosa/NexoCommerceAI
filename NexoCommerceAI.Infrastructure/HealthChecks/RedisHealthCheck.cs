using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace NexoCommerceAI.Infrastructure.HealthChecks;

public class RedisHealthCheck(IConfiguration configuration, ILogger<RedisHealthCheck> logger)
    : IHealthCheck
{
    private readonly string _connectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = await ConnectionMultiplexer.ConnectAsync(_connectionString);
            var database = connection.GetDatabase();
            await database.PingAsync();
            
            return HealthCheckResult.Healthy("Redis is healthy");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Redis health check failed");
            return HealthCheckResult.Unhealthy("Redis is unhealthy", ex);
        }
    }
}