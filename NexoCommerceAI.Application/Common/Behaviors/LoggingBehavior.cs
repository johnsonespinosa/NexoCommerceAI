// Application/Common/Behaviors/LoggingBehavior.cs
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;
using NexoCommerceAI.Application.Common.Attributes;

namespace NexoCommerceAI.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestId = Guid.NewGuid().ToString();
        
        // Log inicio de request
        logger.LogInformation(
            "Processing request {RequestName} [ID: {RequestId}] - Start",
            requestName,
            requestId);
        
        // Log de datos de request en desarrollo
        if (IsSensitiveData(request))
        {
            logger.LogDebug(
                "Request {RequestName} [ID: {RequestId}] Data: {RequestData}",
                requestName,
                requestId,
                JsonSerializer.Serialize(request));
        }
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var response = await next();
            
            stopwatch.Stop();
            
            // Log de éxito
            logger.LogInformation(
                "Processing request {RequestName} [ID: {RequestId}] - Completed in {ElapsedMilliseconds}ms",
                requestName,
                requestId,
                stopwatch.ElapsedMilliseconds);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            // Log de error
            logger.LogError(
                ex,
                "Error processing request {RequestName} [ID: {RequestId}] after {ElapsedMilliseconds}ms",
                requestName,
                requestId,
                stopwatch.ElapsedMilliseconds);
            
            throw;
        }
    }
    
    private bool IsSensitiveData(TRequest request)
    {
        // Verificar si contiene datos sensibles
        var sensitiveProperties = typeof(TRequest).GetProperties()
            .Where(p => p.GetCustomAttributes(typeof(SensitiveDataAttribute), true).Any());
        
        return !sensitiveProperties.Any();
    }
}