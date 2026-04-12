using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Stripe;

namespace NexoCommerceAI.Infrastructure.HealthChecks;

public class StripeHealthCheck(ILogger<StripeHealthCheck> logger) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Verificar configuración de Stripe
            var apiKey = StripeConfiguration.ApiKey;
            
            if (string.IsNullOrEmpty(apiKey) || apiKey == "sk_test_mock_key_for_development")
            {
                return Task.FromResult(HealthCheckResult.Degraded("Stripe is in mock mode"));
            }
            
            return Task.FromResult(HealthCheckResult.Healthy("Stripe is configured"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Stripe health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy("Stripe is unhealthy", ex));
        }
    }
}