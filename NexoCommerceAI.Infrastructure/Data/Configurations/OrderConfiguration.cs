using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        
        builder.HasKey(o => o.Id);
        
        builder.Property(o => o.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.HasIndex(o => o.OrderNumber)
            .IsUnique();
        
        builder.Property(o => o.UserId)
            .IsRequired();
        
        builder.HasIndex(o => o.UserId);
        
        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<int>();
        
        // Money properties
        builder.OwnsOne(o => o.Subtotal, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Subtotal")
                .HasPrecision(18, 2);
            money.Property(m => m.Currency)
                .HasColumnName("SubtotalCurrency")
                .HasMaxLength(3);
        });
        
        builder.OwnsOne(o => o.TaxAmount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("TaxAmount")
                .HasPrecision(18, 2);
            money.Property(m => m.Currency)
                .HasColumnName("TaxCurrency")
                .HasMaxLength(3);
        });
        
        builder.OwnsOne(o => o.ShippingAmount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("ShippingAmount")
                .HasPrecision(18, 2);
            money.Property(m => m.Currency)
                .HasColumnName("ShippingCurrency")
                .HasMaxLength(3);
        });
        
        builder.OwnsOne(o => o.DiscountAmount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("DiscountAmount")
                .HasPrecision(18, 2);
            money.Property(m => m.Currency)
                .HasColumnName("DiscountCurrency")
                .HasMaxLength(3);
        });
        
        builder.OwnsOne(o => o.TotalAmount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("TotalAmount")
                .HasPrecision(18, 2);
            money.Property(m => m.Currency)
                .HasColumnName("TotalCurrency")
                .HasMaxLength(3);
        });
        
        // Addresses
        builder.OwnsOne(o => o.ShippingAddress, address =>
        {
            address.Property(a => a.Street)
                .HasColumnName("ShippingStreet")
                .HasMaxLength(200)
                .IsRequired();
            address.Property(a => a.City)
                .HasColumnName("ShippingCity")
                .HasMaxLength(100)
                .IsRequired();
            address.Property(a => a.State)
                .HasColumnName("ShippingState")
                .HasMaxLength(100);
            address.Property(a => a.ZipCode)
                .HasColumnName("ShippingZipCode")
                .HasMaxLength(20)
                .IsRequired();
            address.Property(a => a.Country)
                .HasColumnName("ShippingCountry")
                .HasMaxLength(100)
                .IsRequired();
        });
        
        builder.OwnsOne(o => o.BillingAddress, address =>
        {
            address.Property(a => a.Street)
                .HasColumnName("BillingStreet")
                .HasMaxLength(200)
                .IsRequired();
            address.Property(a => a.City)
                .HasColumnName("BillingCity")
                .HasMaxLength(100)
                .IsRequired();
            address.Property(a => a.State)
                .HasColumnName("BillingState")
                .HasMaxLength(100);
            address.Property(a => a.ZipCode)
                .HasColumnName("BillingZipCode")
                .HasMaxLength(20)
                .IsRequired();
            address.Property(a => a.Country)
                .HasColumnName("BillingCountry")
                .HasMaxLength(100)
                .IsRequired();
        });
        
        builder.Property(o => o.TrackingNumber)
            .HasMaxLength(100);
        
        builder.Property(o => o.CarrierName)
            .HasMaxLength(100);
        
        builder.Property(o => o.ShippedAt);
        builder.Property(o => o.DeliveredAt);
        builder.Property(o => o.CancelledAt);
        
        builder.Property(o => o.CustomerNotes)
            .HasMaxLength(1000);
        
        builder.Property(o => o.AdminNotes)
            .HasMaxLength(1000);
        
        builder.HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(o => o.StatusHistory)
            .WithOne(h => h.Order)
            .HasForeignKey(h => h.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.CreatedAt);
        builder.HasQueryFilter(o => !o.IsDeleted);
    }
}