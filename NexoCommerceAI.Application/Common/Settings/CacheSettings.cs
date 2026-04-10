namespace NexoCommerceAI.Application.Common.Settings;

public class CacheSettings
{
    public int DefaultExpirationMinutes { get; set; } = 30;
    public int SlidingExpirationMinutes { get; set; } = 10;
    public bool UseRedis { get; set; } = false;
    public string RedisConnectionString { get; set; } = string.Empty;
}