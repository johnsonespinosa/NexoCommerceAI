using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Infrastructure.Data.Configurations;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("ProductImages");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.ImageUrl)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(x => x.PublicId)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(x => x.DisplayOrder)
            .HasDefaultValue(0);
        
        builder.Property(x => x.IsMain)
            .HasDefaultValue(false);
        
        builder.HasOne(x => x.Product)
            .WithMany(p => p.Images)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(x => new { x.ProductId, x.IsMain });
        builder.HasIndex(x => x.DisplayOrder);
    }
}