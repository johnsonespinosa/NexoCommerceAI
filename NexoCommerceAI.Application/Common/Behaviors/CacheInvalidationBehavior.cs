using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Attributes;
using NexoCommerceAI.Application.Common.Interfaces;

namespace NexoCommerceAI.Application.Common.Behaviors;

public class CacheInvalidationBehavior<TRequest, TResponse>(
    ICacheService cacheService,
    ILogger<CacheInvalidationBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Verificar si la request tiene atributo de invalidación
        var invalidateAttribute = typeof(TRequest).GetCustomAttributes(typeof(InvalidateCacheAttribute), true).FirstOrDefault() as InvalidateCacheAttribute;
        
        var response = await next();
        
        if (invalidateAttribute != null && response != null)
        {
            // Invalidar cachés por prefijo
            foreach (var prefix in invalidateAttribute.CacheKeyPrefixes)
            {
                await cacheService.RemoveByPrefixAsync(prefix, cancellationToken);
                logger.LogInformation("Invalidated cache with prefix: {Prefix}", prefix);
            }
        }
        
        return response;
    }
}