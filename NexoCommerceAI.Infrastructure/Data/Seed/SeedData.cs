using Microsoft.EntityFrameworkCore;
using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.Infrastructure.Data.Seed;

public static class SeedData
{
    public static async Task InitializeAsync(ApplicationDbContext context)
    {
        // Roles iniciales
        if (!await context.Roles.AnyAsync())
        {
            var adminRole = Role.Create("Admin", "Administrator with full access to the system");
            var customerRole = Role.Create("Customer", "Regular customer with basic access");
            var managerRole = Role.Create("Manager", "Manager with product management access");
            
            await context.Roles.AddRangeAsync(adminRole, customerRole, managerRole);
            await context.SaveChangesAsync();
        }
        
        // Usuarios iniciales
        if (!await context.Users.AnyAsync())
        {
            // Primero obtenemos los roles desde la base de datos
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            var customerRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Customer");
            
            if (adminRole != null)
            {
                var adminUser = User.Create(
                    "admin",
                    "admin@nexocommerce.com",
                    "Admin123!", // En producción, usa hashing
                    adminRole.Id  // Ahora adminRole.Id tiene un valor válido de la BD
                );
                
                await context.Users.AddAsync(adminUser);
            }
            
            if (customerRole != null)
            {
                var customerUser = User.Create(
                    "customer1",
                    "customer@example.com",
                    "Customer123!",
                    customerRole.Id
                );
                
                await context.Users.AddAsync(customerUser);
            }
            
            await context.SaveChangesAsync();
        }
        
        // Categorías iniciales
        if (!await context.Categories.AnyAsync())
        {
            var categories = new[]
            {
                Category.Create("Electronics", "electronics"),
                Category.Create("Books", "books"),
                Category.Create("Clothing", "clothing"),
                Category.Create("Home & Garden", "home-garden"),
                Category.Create("Sports", "sports")
            };
            
            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }
        
        // Productos iniciales
        if (!await context.Products.AnyAsync())
        {
            var electronicsCategory = await context.Categories
                .FirstOrDefaultAsync(c => c.Slug == "electronics");
            var booksCategory = await context.Categories
                .FirstOrDefaultAsync(c => c.Slug == "books");

            // Actualizar el SeedData para usar el nuevo método Create
            if (electronicsCategory != null)
            {
                var products = new[]
                {
                    Product.Create(
                        name: "Laptop Pro",
                        categoryId: electronicsCategory.Id,
                        slug: "laptop-pro",
                        description: "High-performance laptop for professionals",
                        price: 1299.99m,
                        compareAtPrice: 1499.99m,
                        sku: "LAP-PRO-001",
                        stock: 10,
                        isFeatured: true
                    ),
                    Product.Create(
                        name: "Wireless Mouse",
                        categoryId: electronicsCategory.Id,
                        slug: "wireless-mouse",
                        description: "Ergonomic wireless mouse",
                        price: 29.99m,
                        sku: "MOU-WL-002",
                        stock: 50,
                        isFeatured: false
                    ),
                    Product.Create(
                        name: "Mechanical Keyboard",
                        categoryId: electronicsCategory.Id,
                        slug: "mechanical-keyboard",
                        description: "RGB mechanical keyboard with cherry MX switches",
                        price: 89.99m,
                        compareAtPrice: 129.99m,
                        sku: "KEY-MECH-003",
                        stock: 25,
                        isFeatured: true
                    )
                };

                await context.Products.AddRangeAsync(products);
            }
        }
    }
}