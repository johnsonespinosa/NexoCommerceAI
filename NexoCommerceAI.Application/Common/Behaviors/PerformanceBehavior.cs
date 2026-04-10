// Application/Common/Behaviors/PerformanceBehavior.cs
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace NexoCommerceAI.Application.Common.Behaviors;

public class PerformanceBehavior<TRequest, TResponse>(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly Stopwatch _timer = new();
    private const int PerformanceThreshold = 500; // milisegundos

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _timer.Start();
        
        var response = await next();
        
        _timer.Stop();
        
        var elapsedMilliseconds = _timer.ElapsedMilliseconds;
        
        if (elapsedMilliseconds > PerformanceThreshold)
        {
            var requestName = typeof(TRequest).Name;
            
            logger.LogWarning(
                "Long Running Request: {RequestName} ({ElapsedMilliseconds} milliseconds)",
                requestName,
                elapsedMilliseconds);
        }
        
        return response;
    }
}