using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoCommerceAI.Domain.Entities;
using NexoCommerceAI.Infrastructure.Outbox;

namespace NexoCommerceAI.Infrastructure.Data.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.EventType)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(x => x.Content)
            .IsRequired()
            .HasColumnType("jsonb");
        
        builder.HasIndex(x => x.ProcessedOn);
        builder.HasIndex(x => new { x.IsProcessed, x.OccurredOn });
    }
}