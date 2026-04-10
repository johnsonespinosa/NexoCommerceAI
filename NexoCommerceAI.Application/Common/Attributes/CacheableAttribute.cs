namespace NexoCommerceAI.Application.Common.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class CacheableAttribute(string cacheKeyPrefix) : Attribute
{
    public string CacheKeyPrefix { get; } = cacheKeyPrefix;
    public int ExpirationMinutes { get; set; } = 30;
}