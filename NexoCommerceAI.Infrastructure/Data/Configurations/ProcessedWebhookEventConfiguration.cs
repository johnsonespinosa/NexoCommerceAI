using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Infrastructure.Data.Configurations;

public class ProcessedWebhookEventConfiguration : IEntityTypeConfiguration<ProcessedWebhookEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedWebhookEvent> builder)
    {
        builder.ToTable("ProcessedWebhookEvents");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.EventId)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.HasIndex(x => x.EventId)
            .IsUnique();
        
        builder.Property(x => x.EventType)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(x => x.OrderId)
            .HasMaxLength(50);
        
        builder.HasIndex(x => x.ProcessedAt);
    }
}