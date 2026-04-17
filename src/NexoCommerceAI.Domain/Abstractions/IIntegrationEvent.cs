namespace NexoCommerceAI.Domain.Abstractions;

public interface IIntegrationEvent
{
    DateTime OccurredOnUtc { get; }
}
