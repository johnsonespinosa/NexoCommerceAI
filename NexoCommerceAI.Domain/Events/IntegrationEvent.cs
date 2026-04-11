using NexoCommerceAI.Domain.Common;

namespace NexoCommerceAI.Domain.Events;

public abstract class IntegrationEvent : IIntegrationEvent
{
    protected IntegrationEvent()
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        EventType = GetType().Name;
    }
    
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public string EventType { get; }
}