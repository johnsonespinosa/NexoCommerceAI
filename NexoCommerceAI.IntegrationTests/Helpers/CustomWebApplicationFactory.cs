using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexoCommerceAI.Domain.Entities;
using NexoCommerceAI.Infrastructure.Data;
using NexoCommerceAI.Infrastructure.Services;
using Xunit;

namespace NexoCommerceAI.IntegrationTests.Helpers;

public class CustomWebApplicationFactory(string databaseName) : WebApplicationFactory<Program>, IAsyncLifetime
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        databaseName = $"NexoCommerceAI_Test_{Guid.NewGuid()}";
        
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Add test database
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql($"Host=localhost;Database={databaseName};Username=postgres;Password=postgres");
            });

            // Build service provider
            var sp = services.BuildServiceProvider();
            
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();
            
            // Seed test data
            SeedTestData(db).Wait();
        });
    }

    private async Task SeedTestData(ApplicationDbContext context)
    {
        // Roles
        var adminRole = Role.Create("Admin", "Administrator role");
        var customerRole = Role.Create("Customer", "Regular customer");
        
        await context.Roles.AddRangeAsync(adminRole, customerRole);
        await context.SaveChangesAsync();

        // Categorías
        var electronics = Category.Create("Electronics", "electronics");
        var books = Category.Create("Books", "books");
        
        await context.Categories.AddRangeAsync(electronics, books);
        await context.SaveChangesAsync();

        // Productos
        var products = new[]
        {
            Product.Create("Laptop Pro", electronics.Id, price: 1299.99m, stock: 10, isFeatured: true),
            Product.Create("Wireless Mouse", electronics.Id, price: 29.99m, stock: 50),
            Product.Create("Clean Code", books.Id, price: 45.99m, stock: 100),
            Product.Create("Design Patterns", books.Id, price: 54.99m, stock: 75),
            Product.Create("Out of Stock Product", electronics.Id, price: 99.99m, stock: 0)
        };
        
        await context.Products.AddRangeAsync(products);
        
        // Usuarios
        var passwordHasher = new PasswordHasherService();
        var adminUser = User.Create("admin", "admin@test.com", passwordHasher.Hash("Admin123!"), adminRole.Id);
        var customerUser = User.Create("customer", "customer@test.com", passwordHasher.Hash("Customer123!"), customerRole.Id);
        
        await context.Users.AddRangeAsync(adminUser, customerUser);
        await context.SaveChangesAsync();
    }

    public async Task InitializeAsync()
    {
        // Inicialización adicional si es necesaria
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureDeletedAsync();
    }
}