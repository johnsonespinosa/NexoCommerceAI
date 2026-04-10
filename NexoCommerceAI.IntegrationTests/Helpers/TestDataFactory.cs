using NexoCommerceAI.Domain.Entities;
using NexoCommerceAI.Infrastructure.Services;

namespace NexoCommerceAI.IntegrationTests.Helpers;

public static class TestDataFactory
{
    public static Category CreateCategory(string name, string? slug = null)
    {
        return Category.Create(name, slug);
    }

    public static Product CreateProduct(string name, Guid categoryId, decimal price, int stock, bool isFeatured = false)
    {
        return Product.Create(name, categoryId, price: price, stock: stock, isFeatured: isFeatured);
    }

    public static User CreateUser(string userName, string email, string password, Guid roleId)
    {
        var passwordHasher = new PasswordHasherService();
        return User.Create(userName, email, passwordHasher.Hash(password), roleId);
    }

    public static Role CreateRole(string name, string description)
    {
        return Role.Create(name, description);
    }

    public static List<Product> CreateProductList(Guid categoryId, int count)
    {
        var products = new List<Product>();
        for (int i = 1; i <= count; i++)
        {
            products.Add(CreateProduct($"Product {i}", categoryId, 100m * i, i * 10));
        }
        return products;
    }
}