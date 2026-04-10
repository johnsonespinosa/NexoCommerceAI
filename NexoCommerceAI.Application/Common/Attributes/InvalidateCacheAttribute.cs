// Application/Common/Attributes/InvalidateCacheAttribute.cs
namespace NexoCommerceAI.Application.Common.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class InvalidateCacheAttribute(params string[] cacheKeyPrefixes) : Attribute
{
    public string[] CacheKeyPrefixes { get; } = cacheKeyPrefixes;
}