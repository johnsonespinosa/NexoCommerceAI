using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using NexoCommerceAI.Application.Common.Interfaces;
using System.Text.Json;
using NexoCommerceAI.Application.Common.Settings;

namespace NexoCommerceAI.Infrastructure.Services;

public class RedisCacheService(IDistributedCache distributedCache, IOptions<CacheSettings> settings)
    : ICacheService
{
    private readonly CacheSettings _settings = settings.Value;

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var data = await distributedCache.GetStringAsync(key, cancellationToken);
        return string.IsNullOrEmpty(data) ? default : JsonSerializer.Deserialize<T>(data);
    }
    
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(_settings.DefaultExpirationMinutes),
            SlidingExpiration = TimeSpan.FromMinutes(_settings.SlidingExpirationMinutes)
        };
        
        var data = JsonSerializer.Serialize(value);
        await distributedCache.SetStringAsync(key, data, options, cancellationToken);
    }
    
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await distributedCache.RemoveAsync(key, cancellationToken);
    }
    
    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        // Redis no soporta eliminación por prefijo directamente
        // Se necesita implementar con SCAN o usar librerías adicionales
        await Task.CompletedTask;
    }
    
    public bool TryGet<T>(string key, out T? value)
    {
        value = default;
        var data = distributedCache.GetString(key);
        if (string.IsNullOrEmpty(data))
            return false;
        
        value = JsonSerializer.Deserialize<T>(data);
        return value != null;
    }
    
    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(_settings.DefaultExpirationMinutes),
            SlidingExpiration = TimeSpan.FromMinutes(_settings.SlidingExpirationMinutes)
        };
        
        var data = JsonSerializer.Serialize(value);
        distributedCache.SetString(key, data, options);
    }
    
    public void Remove(string key)
    {
        distributedCache.Remove(key);
    }
}