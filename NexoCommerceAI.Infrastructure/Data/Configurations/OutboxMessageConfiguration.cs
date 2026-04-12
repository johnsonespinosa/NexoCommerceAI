using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoCommerceAI.Domain.Entities;

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
        
        builder.Property(x => x.OccurredOn)
            .IsRequired();
        
        builder.Property(x => x.ProcessedOn);
        
        builder.Property(x => x.Error);
        
        builder.Property(x => x.RetryCount)
            .HasDefaultValue(0);
        
        builder.Property(x => x.AggregateId)
            .HasMaxLength(100);
        
        builder.Property(x => x.UserId)
            .HasMaxLength(100);
        
        builder.Property(x => x.CorrelationId)
            .HasMaxLength(100);
        
        // Índices optimizados
        builder.HasIndex(x => x.ProcessedOn)
            .HasFilter("\"ProcessedOn\" IS NULL");
        
        builder.HasIndex(x => new { x.ProcessedOn, x.OccurredOn });
        
        builder.HasIndex(x => x.EventType);
        
        builder.HasIndex(x => x.AggregateId);
        
        builder.HasIndex(x => x.CorrelationId);
        
        // Índice compuesto para procesamiento
        builder.HasIndex(x => new { x.ProcessedOn, x.RetryCount })
            .HasFilter("\"ProcessedOn\" IS NULL");
    }
}