using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Infrastructure.Data.Configurations;

public class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        builder.ToTable("WishlistItems");
        
        builder.HasKey(i => i.Id);
        
        builder.Property(i => i.WishlistId)
            .IsRequired();
        
        builder.Property(i => i.ProductId)
            .IsRequired();
        
        builder.Property(i => i.ProductName)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(i => i.Price)
            .HasPrecision(18, 2);
        
        builder.Property(i => i.ProductImageUrl)
            .HasMaxLength(500);
        
        builder.Property(i => i.Notes)
            .HasMaxLength(500);
        
        builder.HasIndex(i => i.ProductId);
        builder.HasIndex(i => i.WishlistId);
        
        builder.HasQueryFilter(i => !i.IsDeleted);
    }
}