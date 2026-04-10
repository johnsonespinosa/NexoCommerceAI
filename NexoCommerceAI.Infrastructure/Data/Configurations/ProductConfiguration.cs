using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Infrastructure.Data.Configurations;

public class ProductConfiguration : BaseEntityConfiguration<Product>
{
    public override void Configure(EntityTypeBuilder<Product> builder)
    {
        base.Configure(builder);
        
        builder.ToTable("Products");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(p => p.Slug)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.HasIndex(p => p.Slug)
            .IsUnique();
            
        builder.Property(p => p.Description)
            .HasMaxLength(2000);
            
        builder.Property(p => p.Price)
            .IsRequired()
            .HasPrecision(18, 2);
            
        builder.Property(p => p.CompareAtPrice)
            .HasPrecision(18, 2);
            
        builder.Property(p => p.Sku)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.HasIndex(p => p.Sku)
            .IsUnique();
            
        builder.Property(p => p.Stock)
            .IsRequired()
            .HasDefaultValue(0);
            
        builder.Property(p => p.IsFeatured)
            .HasDefaultValue(false);
            
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Índices para búsquedas frecuentes
        builder.HasIndex(p => p.IsFeatured);
        builder.HasIndex(p => p.IsActive);
        builder.HasIndex(p => new { p.CategoryId, p.IsActive });
        builder.HasIndex(p => p.Price);
        
        // Índices adicionales útiles
        builder.HasIndex(p => new { p.IsActive, p.IsFeatured, p.Price });
        builder.HasIndex(p => new { p.CategoryId, p.IsFeatured, p.IsActive });
        builder.HasIndex(p => new { p.IsActive, p.Stock }); // Para productos activos con stock
        
        // Índice para búsqueda por nombre (si haces búsquedas LIKE)
        builder.HasIndex(p => p.Name)
            .HasMethod("GIN")
            .HasOperators("gin_trgm_ops"); // Para búsqueda de texto parcial (requiere extensión pg_trgm)
        
        // Filtro global
        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}