using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Product> Products { get; }
    DbSet<Category> Categories { get; }
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<ProductImage> ProductImages { get; }
    DbSet<OutboxMessage> OutboxMessages { get; }
    DbSet<OutboxDeadLetter> OutboxDeadLetters { get; }
    public DbSet<Cart> Carts { get; }
    public DbSet<CartItem> CartItems { get; }
    public DbSet<Wishlist> Wishlists { get; }
    public DbSet<WishlistItem> WishlistItems { get; }
    public DbSet<Order> Orders { get; }
    public DbSet<OrderItem> OrderItems { get; }
    public DbSet<OrderStatusHistory> OrderStatusHistories { get; }
    public DbSet<PaymentTransaction> PaymentTransactions { get; }
    public DbSet<PaymentHistory> PaymentHistories { get; }
    
    /// <summary>
    /// Acceso a funcionalidades de base de datos como transacciones y estrategias de ejecución
    /// </summary>
    DatabaseFacade Database { get; }
    
    /// <summary>
    /// Tracker de cambios de Entity Framework
    /// </summary>
    ChangeTracker ChangeTracker { get; }
    
    /// <summary>
    /// Guarda los cambios en la base de datos
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}