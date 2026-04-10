using MediatR;
using Microsoft.Extensions.Logging;
using NexoCommerceAI.Application.Common.Interfaces;
using System.Text.Json;
using NexoCommerceAI.Application.Common.Attributes;

namespace NexoCommerceAI.Application.Common.Behaviors;

public class CachingBehavior<TRequest, TResponse>(
    ICacheService cacheService,
    ILogger<CachingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Verificar si la request tiene atributo de caché
        var cacheableAttribute = typeof(TRequest).GetCustomAttributes(typeof(CacheableAttribute), true).FirstOrDefault() as CacheableAttribute;
        
        if (cacheableAttribute == null)
            return await next();
        
        var cacheKey = GenerateCacheKey(request, cacheableAttribute.CacheKeyPrefix);
        
        // Intentar obtener del caché
        var cachedResponse = await cacheService.GetAsync<TResponse>(cacheKey, cancellationToken);
        if (cachedResponse != null)
        {
            logger.LogInformation("Cache hit for key: {CacheKey}", cacheKey);
            return cachedResponse;
        }
        
        logger.LogInformation("Cache miss for key: {CacheKey}", cacheKey);
        
        // Ejecutar request y almacenar en caché
        var response = await next();
        
        if (response != null)
        {
            await cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(cacheableAttribute.ExpirationMinutes), cancellationToken);
        }
        
        return response;
    }
    
    private string GenerateCacheKey(TRequest request, string prefix)
    {
        var serializedRequest = JsonSerializer.Serialize(request);
        var hash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(serializedRequest));
        return $"{prefix}_{hash[..Math.Min(50, hash.Length)]}";
    }
}