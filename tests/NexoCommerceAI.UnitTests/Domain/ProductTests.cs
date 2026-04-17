using NexoCommerceAI.Domain.Entities;

namespace NexoCommerceAI.UnitTests.Domain;

public class ProductTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateProduct()
    {
        var categoryId = Guid.NewGuid();

        var product = Product.Create(
            name: "Nike Air Max",
            categoryId: categoryId,
            description: "Running shoes",
            price: 199.99m,
            stock: 10);

        Assert.Equal("Nike Air Max", product.Name);
        Assert.Equal(categoryId, product.CategoryId);
        Assert.Equal(199.99m, product.Price);
        Assert.Equal(10, product.Stock);
        Assert.False(string.IsNullOrWhiteSpace(product.Slug));
        Assert.False(string.IsNullOrWhiteSpace(product.Sku));
    }

    [Fact]
    public void Create_WithInvalidPrice_ShouldThrow()
    {
        var categoryId = Guid.NewGuid();

        var action = () => Product.Create(
            name: "Test Product",
            categoryId: categoryId,
            price: 0,
            stock: 1);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void DecreaseStock_WhenStockReachesZero_ShouldRaiseOutOfStockEvent()
    {
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", categoryId, price: 20, stock: 3);

        product.DecreaseStock(3);

        Assert.Equal(0, product.Stock);
        Assert.Contains(product.DomainEvents, e => e.GetType().Name == "OutOfStockEvent");
    }
}
