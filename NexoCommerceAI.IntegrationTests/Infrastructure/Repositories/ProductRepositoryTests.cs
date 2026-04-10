using Microsoft.EntityFrameworkCore;
using NexoCommerceAI.Domain.Entities;
using NexoCommerceAI.Infrastructure.Data.Repositories;
using NexoCommerceAI.IntegrationTests.Helpers;
using Xunit;

namespace NexoCommerceAI.IntegrationTests.Infrastructure.Repositories;

public class ProductRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly ProductRepository _repository;

    public ProductRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new ProductRepository(_fixture.Context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddProductToDatabase()
    {
        // Arrange
        var category = await _fixture.Context.Categories.FirstAsync();
        var product = Product.Create("New Product", category.Id, price: 99.99m, stock: 20);

        // Act
        await _repository.AddAsync(product);
        await _repository.SaveChangesAsync();

        // Assert
        var savedProduct = await _fixture.Context.Products.FindAsync(product.Id);
        Assert.NotNull(savedProduct);
        Assert.Equal(product.Name, savedProduct.Name);
        Assert.Equal(product.Price, savedProduct.Price);
    }

    [Fact]
    public async Task GetByIdWithCategoryAsync_ShouldReturnProductWithCategory()
    {
        // Arrange
        var product = await _fixture.Context.Products
            .Include(p => p.Category)
            .FirstAsync();

        // Act
        var result = await _repository.GetByIdWithCategoryAsync(product.Id);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Category);
        Assert.Equal(product.CategoryId, result.CategoryId);
    }

    [Fact]
    public async Task GetPaginatedProductsAsync_ShouldReturnCorrectPage()
    {
        // Act
        var page1 = await _repository.GetPaginatedProductsAsync(1, 2);
        var page2 = await _repository.GetPaginatedProductsAsync(2, 2);

        // Assert
        Assert.Equal(2, page1.Count);
        Assert.Equal(2, page2.Count);
        Assert.NotEqual(page1[0].Id, page2[0].Id);
    }

    [Fact]
    public async Task GetFeaturedProductsAsync_ShouldReturnOnlyFeaturedProducts()
    {
        // Act
        var featured = await _repository.GetFeaturedProductsAsync();

        // Assert
        Assert.All(featured, p => Assert.True(p.IsFeatured));
    }

    [Fact]
    public async Task SearchProductsAsync_ShouldReturnMatchingProducts()
    {
        // Act
        var results = await _repository.SearchProductsAsync("Laptop", 10);

        // Assert
        Assert.Contains(results, p => p.Name.Contains("Laptop"));
    }

    [Fact]
    public async Task DecreaseStockAsync_ShouldDecreaseStockWhenSufficient()
    {
        // Arrange
        var product = await _fixture.Context.Products.FirstAsync(p => p.Stock >= 10);
        var originalStock = product.Stock;
        const int quantity = 3;

        // Act
        var result = await _repository.DecreaseStockAsync(product.Id, quantity);

        // Assert
        Assert.True(result);
        var updatedProduct = await _fixture.Context.Products.FindAsync(product.Id);
        Assert.Equal(originalStock - quantity, updatedProduct!.Stock);
    }

    [Fact]
    public async Task DecreaseStockAsync_ShouldReturnFalseWhenInsufficientStock()
    {
        // Arrange
        var product = await _fixture.Context.Products.FirstAsync(p => p.Stock == 0);
        const int quantity = 5;

        // Act
        var result = await _repository.DecreaseStockAsync(product.Id, quantity);

        // Assert
        Assert.False(result);
    }
}