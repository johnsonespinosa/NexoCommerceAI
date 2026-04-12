using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Infrastructure.Data.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts");
        
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.UserId)
            .IsRequired();
        
        builder.Property(c => c.LastUpdatedAt);
        
        builder.Property(c => c.IsAbandoned);
        
        builder.Property(c => c.AbandonedAt);
        
        // Ignorar propiedades calculadas (solo getter)
        builder.Ignore(c => c.TotalAmount);
        builder.Ignore(c => c.TotalItems);
        
        builder.HasIndex(c => c.UserId)
            .IsUnique();
        
        builder.HasIndex(c => c.IsAbandoned);
        
        builder.HasMany(c => c.Items)
            .WithOne(i => i.Cart)
            .HasForeignKey(i => i.CartId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}