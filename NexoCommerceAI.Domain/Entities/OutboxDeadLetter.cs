namespace NexoCommerceAI.Domain.Entities;

public class OutboxDeadLetter
{
    public Guid Id { get; set; }
    public Guid OriginalMessageId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
    public DateTime OccurredOn { get; set; }
    public DateTime MovedToDeadLetterAt { get; set; }
}