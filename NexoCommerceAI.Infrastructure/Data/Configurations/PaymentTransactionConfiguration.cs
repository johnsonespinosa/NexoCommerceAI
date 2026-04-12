using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Infrastructure.Data.Configurations;

public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.ToTable("PaymentTransactions");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.OrderId)
            .IsRequired();
        
        builder.Property(p => p.PaymentMethod)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(p => p.TransactionId)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.HasIndex(p => p.TransactionId)
            .IsUnique();
        
        builder.Property(p => p.Amount)
            .HasPrecision(18, 2);
        
        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<int>();
        
        builder.Property(p => p.ProviderResponse)
            .HasColumnType("jsonb");
        
        builder.Property(p => p.Last4)
            .HasMaxLength(4);
        
        builder.Property(p => p.CardType)
            .HasMaxLength(50);
        
        builder.Property(p => p.CardHolderName)
            .HasMaxLength(100);
        
        builder.Property(p => p.RequestedAt);
        builder.Property(p => p.CompletedAt);
        
        builder.HasIndex(p => p.OrderId);
        builder.HasIndex(p => p.Status);
        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}