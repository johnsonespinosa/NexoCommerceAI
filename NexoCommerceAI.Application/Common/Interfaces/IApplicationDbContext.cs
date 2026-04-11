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