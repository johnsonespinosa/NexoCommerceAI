using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NexoCommerceAI.Domain.Entities;
using NexoCommerceAI.Infrastructure.Data;
using NexoCommerceAI.Infrastructure.Services;
using Xunit;

namespace NexoCommerceAI.IntegrationTests.Helpers;

public class DatabaseFixture : IAsyncLifetime
{
    public ApplicationDbContext Context { get; private set; }
    public IConfiguration Configuration { get; private set; }

    public DatabaseFixture()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.test.json")
            .Build();

        Configuration = configuration;

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(Configuration.GetConnectionString("TestConnection"))
            .Options;

        Context = new ApplicationDbContext(options);
    }

    public async Task InitializeAsync()
    {
        await Context.Database.EnsureDeletedAsync();
        await Context.Database.EnsureCreatedAsync();
        await SeedTestData();
    }

    public async Task DisposeAsync()
    {
        await Context.Database.EnsureDeletedAsync();
        await Context.DisposeAsync();
    }

    private async Task SeedTestData()
    {
        // Roles
        var adminRole = Role.Create("Admin", "Administrator role");
        var customerRole = Role.Create("Customer", "Regular customer");
        
        await Context.Roles.AddRangeAsync(adminRole, customerRole);
        await Context.SaveChangesAsync();

        // Categorías
        var electronics = Category.Create("Electronics", "electronics");
        var books = Category.Create("Books", "books");
        
        await Context.Categories.AddRangeAsync(electronics, books);
        await Context.SaveChangesAsync();

        // Productos
        var products = new[]
        {
            Product.Create("Laptop Pro", electronics.Id, price: 1299.99m, stock: 10, isFeatured: true),
            Product.Create("Wireless Mouse", electronics.Id, price: 29.99m, stock: 50),
            Product.Create("Clean Code", books.Id, price: 45.99m, stock: 100),
            Product.Create("Design Patterns", books.Id, price: 54.99m, stock: 75)
        };
        
        await Context.Products.AddRangeAsync(products);
        
        // Usuarios
        var passwordHasher = new PasswordHasherService();
        var adminUser = User.Create("admin", "admin@test.com", passwordHasher.Hash("Admin123!"), adminRole.Id);
        var customerUser = User.Create("customer", "customer@test.com", passwordHasher.Hash("Customer123!"), customerRole.Id);
        
        await Context.Users.AddRangeAsync(adminUser, customerUser);
        await Context.SaveChangesAsync();
    }
}