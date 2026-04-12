using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Infrastructure.Data.Configurations;

public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("OrderStatusHistory");
        
        builder.HasKey(h => h.Id);
        
        builder.Property(h => h.OrderId)
            .IsRequired();
        
        builder.Property(h => h.OldStatus)
            .IsRequired()
            .HasConversion<int>();
        
        builder.Property(h => h.NewStatus)
            .IsRequired()
            .HasConversion<int>();
        
        builder.Property(h => h.ChangedBy)
            .HasMaxLength(100);
        
        builder.Property(h => h.Comment)
            .HasMaxLength(500);
        
        builder.Property(h => h.ChangedAt)
            .IsRequired();
        
        builder.HasIndex(h => h.OrderId);
        builder.HasIndex(h => h.ChangedAt);
        builder.HasQueryFilter(h => !h.IsDeleted);
    }
}