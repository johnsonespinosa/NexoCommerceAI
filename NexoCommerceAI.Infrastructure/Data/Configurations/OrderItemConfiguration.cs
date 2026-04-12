using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Infrastructure.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");
        
        builder.HasKey(i => i.Id);
        
        builder.Property(i => i.OrderId)
            .IsRequired();
        
        builder.Property(i => i.ProductId)
            .IsRequired();
        
        builder.HasIndex(i => i.ProductId);
        
        builder.Property(i => i.ProductName)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(i => i.ProductSku)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(i => i.UnitPrice)
            .HasPrecision(18, 2);
        
        builder.Property(i => i.Quantity)
            .IsRequired();
        
        builder.Property(i => i.ProductImageUrl)
            .HasMaxLength(500);
        
        builder.Property(i => i.ProductSnapshot)
            .HasColumnType("jsonb");
        
        builder.Ignore(i => i.TotalPrice);
        
        builder.HasIndex(i => i.OrderId);
        builder.HasQueryFilter(i => !i.IsDeleted);
    }
}