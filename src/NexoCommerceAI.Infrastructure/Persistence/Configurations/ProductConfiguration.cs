using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(250).IsRequired();
        builder.Property(p => p.Slug).HasMaxLength(300).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(2000);
        builder.Property(p => p.Price).HasPrecision(18, 2).IsRequired();
        builder.Property(p => p.CompareAtPrice).HasPrecision(18, 2);
        builder.Property(p => p.Sku).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Stock).IsRequired();
        builder.Property(p => p.IsFeatured).IsRequired();
        builder.Property(p => p.CategoryId).IsRequired();
        builder.Property(p => p.CreatedBy).HasMaxLength(120);
        builder.Property(p => p.UpdatedBy).HasMaxLength(120);

        builder.HasIndex(p => p.Sku).IsUnique();
        builder.HasIndex(p => p.Slug).IsUnique();

        // Category relation is intentionally not enforced yet to keep the first slice independent.
        builder.Ignore(p => p.Category);
        builder.HasMany(p => p.Images).WithOne().HasForeignKey(i => i.ProductId);
    }
}
