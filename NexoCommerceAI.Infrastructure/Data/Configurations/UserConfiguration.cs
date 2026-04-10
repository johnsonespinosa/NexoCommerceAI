using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Infrastructure.Data.Configurations;

public class UserConfiguration : BaseEntityConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);
        
        builder.ToTable("Users");
        
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.HasIndex(u => u.UserName)
            .IsUnique();
            
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.HasIndex(u => u.Email)
            .IsUnique();
            
        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.Property(u => u.RefreshToken)
            .HasMaxLength(500);
            
        // Relaciones
        builder.HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Filtros globales
        builder.HasQueryFilter(u => !u.IsDeleted);
        
        // Índices
        builder.HasIndex(u => u.IsActive);
        builder.HasIndex(u => new { u.RoleId, u.IsActive });
    }
}

