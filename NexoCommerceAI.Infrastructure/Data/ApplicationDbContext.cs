using Microsoft.EntityFrameworkCore;
using NexoCommerceAI.Domain.Entities;
using NexoCommerceAI.Infrastructure.Data.Interceptors;

namespace NexoCommerceAI.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    private readonly AuditableEntityInterceptor? _auditableInterceptor;
    
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    
    // Constructor para uso normal (sin interceptor)
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }
    
    // Constructor para inyección de interceptor
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        AuditableEntityInterceptor auditableInterceptor) 
        : base(options)
    {
        _auditableInterceptor = auditableInterceptor;
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_auditableInterceptor is not null)
        {
            optionsBuilder.AddInterceptors(_auditableInterceptor);
        }
        base.OnConfiguring(optionsBuilder);
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}