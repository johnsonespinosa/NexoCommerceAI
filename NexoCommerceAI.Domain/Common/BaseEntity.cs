using System.ComponentModel.DataAnnotations.Schema;
using MediatR;

namespace NexoCommerceAI.Domain.Common;

public abstract class BaseEntity
{
    private readonly List<INotification> _domainEvents = [];
    private readonly List<IIntegrationEvent> _integrationEvents = [];

    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    
    [NotMapped]
    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();
    
    [NotMapped]
    public IReadOnlyCollection<IIntegrationEvent> IntegrationEvents => _integrationEvents.AsReadOnly();

    public void AddDomainEvent(INotification eventItem)
    {
        _domainEvents.Add(eventItem);
    }

    public void RemoveDomainEvent(INotification eventItem)
    {
        _domainEvents.Remove(eventItem);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
    
    public void AddIntegrationEvent(IIntegrationEvent eventItem)
    {
        _integrationEvents.Add(eventItem);
    }

    public void RemoveIntegrationEvent(IIntegrationEvent eventItem)
    {
        _integrationEvents.Remove(eventItem);
    }

    public void ClearIntegrationEvents()
    {
        _integrationEvents.Clear();
    }
    
    // Método útil para restaurar soft deleted
    public void Restore()
    {
        IsDeleted = false;
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}