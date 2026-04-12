using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Infrastructure.Data.Configurations;

public class WishlistConfiguration : IEntityTypeConfiguration<Wishlist>
{
    public void Configure(EntityTypeBuilder<Wishlist> builder)
    {
        builder.ToTable("Wishlists");
        
        builder.HasKey(w => w.Id);
        
        builder.Property(w => w.UserId)
            .IsRequired();
        
        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(w => w.IsDefault);
        
        // Ignorar propiedad calculada
        builder.Ignore(w => w.TotalItems);
        
        builder.HasIndex(w => new { w.UserId, w.IsDefault });
        
        builder.HasMany(w => w.Items)
            .WithOne(i => i.Wishlist)
            .HasForeignKey(i => i.WishlistId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasQueryFilter(w => !w.IsDeleted);
    }
}