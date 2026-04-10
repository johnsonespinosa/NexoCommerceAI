namespace NexoCommerceAI.Application.Common.Interfaces;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
    bool TryGet<T>(string key, out T? value);
    void Set<T>(string key, T value, TimeSpan? expiration = null);
    void Remove(string key);
}