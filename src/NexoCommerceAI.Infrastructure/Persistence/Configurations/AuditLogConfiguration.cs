using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Infrastructure.Persistence.Configurations;

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EntityType).IsRequired().HasMaxLength(250);
        builder.Property(x => x.EntityId).IsRequired().HasMaxLength(200);

        builder.Property(x => x.Action).IsRequired().HasConversion<string>().HasMaxLength(50);

        // Use 'text' for PostgreSQL to store JSON payloads
        builder.Property(x => x.OldValues).HasColumnType("text");
        builder.Property(x => x.NewValues).HasColumnType("text");

        builder.Property(x => x.ChangedBy).IsRequired().HasMaxLength(200);
        builder.Property(x => x.ChangedAt).IsRequired();

        builder.Property(x => x.IpAddress).HasMaxLength(45);
        builder.Property(x => x.UserAgent).HasMaxLength(1000);

        builder.HasIndex(x => x.EntityType);
        builder.HasIndex(x => x.EntityId);
        builder.HasIndex(x => x.ChangedAt);
        builder.HasIndex(x => new { x.EntityType, x.EntityId, x.ChangedAt });
    }
}
