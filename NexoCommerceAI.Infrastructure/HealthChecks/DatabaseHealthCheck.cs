using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace NexoCommerceAI.Infrastructure.HealthChecks;

public class DatabaseHealthCheck(IConfiguration configuration, ILogger<DatabaseHealthCheck> logger)
    : IHealthCheck
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            await command.ExecuteScalarAsync(cancellationToken);
            
            return HealthCheckResult.Healthy("Database is healthy");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy("Database is unhealthy", ex);
        }
    }
}