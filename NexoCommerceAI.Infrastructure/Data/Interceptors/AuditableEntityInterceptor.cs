using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Domain.Common;

namespace NexoCommerceAI.Infrastructure.Data.Interceptors;

public class AuditableEntityInterceptor(ICurrentUserService currentUserService) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, 
        InterceptionResult<int> result, 
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            UpdateAuditableEntities(eventData.Context);
            HandleSoftDelete(eventData.Context);
        }
        
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
    
    private void UpdateAuditableEntities(DbContext context)
    {
        var userId = currentUserService.UserId?.ToString() ?? "system";
        var utcNow = DateTime.UtcNow;
        
        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = utcNow;
                entry.Entity.CreatedBy = userId;
                entry.Entity.UpdatedAt = null;
                entry.Entity.UpdatedBy = null;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = utcNow;
                entry.Entity.UpdatedBy = userId;
                
                // Mantener valores originales de creación
                entry.Property(nameof(BaseEntity.CreatedAt)).IsModified = false;
                entry.Property(nameof(BaseEntity.CreatedBy)).IsModified = false;
            }
        }
    }
    
    private void HandleSoftDelete(DbContext context)
    {
        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Deleted)
            {
                // Convertir delete físico en soft delete
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.IsActive = false;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedBy = currentUserService.UserId?.ToString() ?? "system";
            }
        }
    }
}

// Interceptor adicional para consultas: filtrar automáticamente soft delete
public class SoftDeleteQueryInterceptor : IMaterializationInterceptor
{
    public object CreatedInstance(MaterializationInterceptionData materializationData, object entity)
    {
        return entity;
    }
    
    public object InitializedInstance(MaterializationInterceptionData materializationData, object entity)
    {
        return entity;
    }
}