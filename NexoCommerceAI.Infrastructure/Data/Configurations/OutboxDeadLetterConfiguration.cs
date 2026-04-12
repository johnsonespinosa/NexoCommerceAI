using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Infrastructure.Data.Configurations;

public class OutboxDeadLetterConfiguration : IEntityTypeConfiguration<OutboxDeadLetter>
{
    public void Configure(EntityTypeBuilder<OutboxDeadLetter> builder)
    {
        builder.ToTable("OutboxDeadLetters");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.OriginalMessageId)
            .IsRequired();
        
        builder.Property(x => x.EventType)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(x => x.Content)
            .IsRequired()
            .HasColumnType("jsonb");
        
        builder.Property(x => x.Error)
            .IsRequired()
            .HasMaxLength(2000);
        
        builder.Property(x => x.OccurredOn)
            .IsRequired();
        
        builder.Property(x => x.MovedToDeadLetterAt)
            .IsRequired();
        
        builder.HasIndex(x => x.OriginalMessageId);
        builder.HasIndex(x => x.MovedToDeadLetterAt);
        builder.HasIndex(x => x.EventType);
    }
}