namespace NexoCommerceAI.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    
    // Método útil para restaurar soft deleted
    public void Restore()
    {
        IsDeleted = false;
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}