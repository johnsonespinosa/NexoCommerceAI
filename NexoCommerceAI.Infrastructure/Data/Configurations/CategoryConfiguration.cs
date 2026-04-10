using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Infrastructure.Data.Configurations;

public class CategoryConfiguration : BaseEntityConfiguration<Category>
{
    public override void Configure(EntityTypeBuilder<Category> builder)
    {
        base.Configure(builder);
        
        builder.ToTable("Categories");
        
        builder.HasKey(category => category.Id);
        
        builder.Property(category => category.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(category => category.Slug)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.HasIndex(category => category.Slug)
            .IsUnique();
            
        // Filtro global
        builder.HasQueryFilter(category => !category.IsDeleted);
        
        // Índice para búsqueda
        builder.HasIndex(category => category.IsActive);
    }
}