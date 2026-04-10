using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NexoCommerceAI.Application.Common.Interfaces;
using System.Text.Json;
using NexoCommerceAI.Application.Common.Settings;

namespace NexoCommerceAI.Infrastructure.Services;

public class MemoryCacheService(IMemoryCache cache, IOptions<CacheSettings> settings) : ICacheService
{
    private readonly CacheSettings _settings = settings.Value;
    private static readonly SemaphoreSlim Semaphore = new(1, 1);

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        return cache.TryGetValue(key, out T? value) ? value : default;
    }
    
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(_settings.DefaultExpirationMinutes),
            SlidingExpiration = TimeSpan.FromMinutes(_settings.SlidingExpirationMinutes),
            Priority = CacheItemPriority.Normal
        };
        
        cache.Set(key, value, cacheOptions);
    }
    
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        cache.Remove(key);
    }
    
    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        // Nota: IMemoryCache no soporta eliminación por prefijo nativamente
        // Esta es una implementación básica, para algo más robusto considera Redis
        await Task.CompletedTask;
    }
    
    public bool TryGet<T>(string key, out T? value)
    {
        return cache.TryGetValue(key, out value);
    }
    
    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(_settings.DefaultExpirationMinutes),
            SlidingExpiration = TimeSpan.FromMinutes(_settings.SlidingExpirationMinutes),
            Priority = CacheItemPriority.Normal
        };
        
        cache.Set(key, value, cacheOptions);
    }
    
    public void Remove(string key)
    {
        cache.Remove(key);
    }
}