using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Infrastructure.Persistence.Configurations;

internal sealed class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("product_images");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.ProductId).IsRequired();
        builder.Property(i => i.ImageUrl).HasMaxLength(1000).IsRequired();
        builder.Property(i => i.PublicId).HasMaxLength(300);
        builder.Property(i => i.DisplayOrder).IsRequired();
        builder.Property(i => i.IsMain).IsRequired();
        builder.Property(i => i.CreatedBy).HasMaxLength(120);
        builder.Property(i => i.UpdatedBy).HasMaxLength(120);
    }
}
