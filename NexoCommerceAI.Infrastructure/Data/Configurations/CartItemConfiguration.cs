using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Infrastructure.Data.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");
        
        builder.HasKey(i => i.Id);
        
        builder.Property(i => i.CartId)
            .IsRequired();
        
        builder.Property(i => i.ProductId)
            .IsRequired();
        
        builder.Property(i => i.ProductName)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(i => i.UnitPrice)
            .HasPrecision(18, 2);
        
        builder.Property(i => i.Quantity)
            .IsRequired();
        
        builder.Property(i => i.ProductImageUrl)
            .HasMaxLength(500);
        
        // Ignorar propiedad calculada
        builder.Ignore(i => i.TotalPrice);
        
        builder.HasIndex(i => i.ProductId);
        builder.HasIndex(i => i.CartId);
        
        builder.HasQueryFilter(i => !i.IsDeleted);
    }
}