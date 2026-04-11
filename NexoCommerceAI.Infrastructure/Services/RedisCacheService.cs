using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Common.Settings;
using StackExchange.Redis;

namespace NexoCommerceAI.Infrastructure.Services;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly CacheSettings _settings;
    private readonly ILogger<RedisCacheService> _logger;
    
    public RedisCacheService(
        IDistributedCache distributedCache, 
        IOptions<CacheSettings> settings,
        ILogger<RedisCacheService> logger)
    {
        _distributedCache = distributedCache;
        _settings = settings.Value;
        _logger = logger;
    }
    
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await _distributedCache.GetStringAsync(key, cancellationToken);
            if (string.IsNullOrEmpty(data))
                return default;
            
            return JsonSerializer.Deserialize<T>(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis get error for key: {Key}", key);
            return default;
        }
    }
    
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(_settings.DefaultExpirationMinutes),
                SlidingExpiration = TimeSpan.FromMinutes(_settings.SlidingExpirationMinutes)
            };
            
            var data = JsonSerializer.Serialize(value);
            await _distributedCache.SetStringAsync(key, data, options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis set error for key: {Key}", key);
        }
    }
    
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _distributedCache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis remove error for key: {Key}", key);
        }
    }
    
    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        // Redis no soporta eliminación por prefijo nativamente
        // Esta es una implementación básica usando SCAN
        try
        {
            var server = GetServer();
            var keys = server.Keys(pattern: $"{prefix}*").ToArray();
            
            foreach (var key in keys)
            {
                await _distributedCache.RemoveAsync(key, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis remove by prefix error for prefix: {Prefix}", prefix);
        }
    }
    
    public bool TryGet<T>(string key, out T? value)
    {
        value = default;
        try
        {
            var data = _distributedCache.GetString(key);
            if (string.IsNullOrEmpty(data))
                return false;
            
            value = JsonSerializer.Deserialize<T>(data);
            return value != null;
        }
        catch
        {
            return false;
        }
    }
    
    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(_settings.DefaultExpirationMinutes),
                SlidingExpiration = TimeSpan.FromMinutes(_settings.SlidingExpirationMinutes)
            };
            
            var data = JsonSerializer.Serialize(value);
            _distributedCache.SetString(key, data, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis set error for key: {Key}", key);
        }
    }
    
    public void Remove(string key)
    {
        try
        {
            _distributedCache.Remove(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis remove error for key: {Key}", key);
        }
    }
    
    private IServer GetServer()
    {
        var connection = ConnectionMultiplexer.Connect(_settings.RedisConnectionString);
        return connection.GetServer(connection.GetEndPoints().First());
    }
}