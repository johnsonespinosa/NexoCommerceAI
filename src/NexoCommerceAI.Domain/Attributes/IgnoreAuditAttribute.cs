using System;

namespace NexoCommerceAI.Domain.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
public sealed class IgnoreAuditAttribute : Attribute
{
}
