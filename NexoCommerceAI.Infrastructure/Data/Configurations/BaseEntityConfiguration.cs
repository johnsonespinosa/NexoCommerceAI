using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoCommerceAI.Domain.Common;

namespace NexoCommerceAI.Infrastructure.Data.Configurations;

// Configuración base para todas las entidades
public class BaseEntityConfiguration<T> : IEntityTypeConfiguration<T> where T : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.Property(entity => entity.Id)
            .HasDefaultValueSql("gen_random_uuid()");
            
        builder.Property(entity => entity.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();
            
        builder.Property(entity => entity.IsActive)
            .HasDefaultValue(true);
            
        builder.Property(entity => entity.IsDeleted)
            .HasDefaultValue(false);
            
        builder.HasIndex(entity => entity.IsDeleted);
        builder.HasIndex(entity => new { entity.IsActive, entity.IsDeleted });
    }
}