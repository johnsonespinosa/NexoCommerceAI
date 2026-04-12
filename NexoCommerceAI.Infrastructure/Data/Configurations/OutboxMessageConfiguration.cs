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
        
        builder.Property(x => x.RetryCount);
        
        // Ignorar propiedad calculada
        builder.Ignore(x => x.IsProcessed);
        
        // Índices - NO usar x.IsProcessed porque fue ignorado
        builder.HasIndex(x => x.ProcessedOn);
        // Este índice no se puede crear porque IsProcessed no está mapeado
        // builder.HasIndex(x => new { x.IsProcessed, x.OccurredOn });
    }
}