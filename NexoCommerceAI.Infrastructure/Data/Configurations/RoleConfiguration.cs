using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Infrastructure.Data.Configurations;

public class RoleConfiguration : BaseEntityConfiguration<Role>
{
    public override void Configure(EntityTypeBuilder<Role> builder)
    {
        base.Configure(builder);
        
        builder.ToTable("Roles");
        
        builder.HasKey(r => r.Id);
        
        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.HasIndex(r => r.Name)
            .IsUnique();
            
        builder.Property(r => r.Description)
            .IsRequired()
            .HasMaxLength(200);
            
        // Filtros globales
        builder.HasQueryFilter(r => !r.IsDeleted);
        
        // Índices
        builder.HasIndex(r => r.IsActive);
    }
}