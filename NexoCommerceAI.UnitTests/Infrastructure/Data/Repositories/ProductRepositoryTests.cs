using Microsoft.EntityFrameworkCore;
using NexoCommerceAI.Domain.Entities;
using NexoCommerceAI.Infrastructure.Data;
using NexoCommerceAI.Infrastructure.Data.Repositories;
using Xunit;

namespace NexoCommerceAI.UnitTests.Infrastructure.Data.Repositories;

public class ProductRepositoryTests
{
    private async Task<ApplicationDbContext> GetDbContextAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        var context = new ApplicationDbContext(options);
        await SeedDataAsync(context);
        return context;
    }
    
    private async Task SeedDataAsync(ApplicationDbContext context)
    {
        var category = Category.Create("Electronics");
        await context.Categories.AddAsync(category);
        await context.SaveChangesAsync();
        
        var products = new[]
        {
            Product.Create("Laptop", category.Id, price: 1000m, stock: 10, isFeatured: true),
            Product.Create("Mouse", category.Id, price: 30m, stock: 50),
            Product.Create("Keyboard", category.Id, price: 80m, stock: 3),
            Product.Create("Monitor", category.Id, price: 300m, stock: 0),
        };
        
        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();
    }
    
    [Fact]
    public async Task ExistBySkuAsync_WithExistingSku_ShouldReturnTrue()
    {
        // Arrange
        var context = await GetDbContextAsync();
        var repository = new ProductRepository(context);
        var product = await context.Products.FirstAsync();
        
        // Act
        var exists = await repository.ExistBySkuAsync(product.Sku);
        
        // Assert
        Assert.True(exists);
    }
    
    [Fact]
    public async Task ExistBySkuAsync_WithNonExistingSku_ShouldReturnFalse()
    {
        // Arrange
        var context = await GetDbContextAsync();
        var repository = new ProductRepository(context);
        
        // Act
        var exists = await repository.ExistBySkuAsync("NON-EXISTENT-SKU");
        
        // Assert
        Assert.False(exists);
    }
    
    [Fact]
    public async Task ExistBySkuAsync_WithNullSku_ShouldReturnFalse()
    {
        // Arrange
        var context = await GetDbContextAsync();
        var repository = new ProductRepository(context);
        
        // Act
        var exists = await repository.ExistBySkuAsync(null!);
        
        // Assert
        Assert.False(exists);
    }
    
    [Fact]
    public async Task ExistBySlugAsync_WithExistingSlug_ShouldReturnTrue()
    {
        // Arrange
        var context = await GetDbContextAsync();
        var repository = new ProductRepository(context);
        var product = await context.Products.FirstAsync();
        
        // Act
        var exists = await repository.ExistBySlugAsync(product.Slug);
        
        // Assert
        Assert.True(exists);
    }
    
    [Fact]
    public async Task GetFeaturedProductsAsync_ShouldReturnOnlyFeaturedProducts()
    {
        // Arrange
        var context = await GetDbContextAsync();
        var repository = new ProductRepository(context);
        
        // Act
        var featured = await repository.GetFeaturedProductsAsync();
        
        // Assert
        Assert.All(featured, p => Assert.True(p.IsFeatured));
    }
    
    [Fact]
    public async Task GetProductsOnSaleAsync_ShouldReturnProductsWithCompareAtPrice()
    {
        // Arrange
        var context = await GetDbContextAsync();
        var repository = new ProductRepository(context);
        var product = await context.Products.FirstAsync();
        product.UpdatePrice(800m, 1000m);
        await context.SaveChangesAsync();
        
        // Act
        var onSale = await repository.GetProductsOnSaleAsync();
        
        // Assert
        Assert.All(onSale, p => Assert.True(p.CompareAtPrice > p.Price));
    }
    
    [Fact]
    public async Task UpdateStockAsync_WithValidProduct_ShouldUpdateStock()
    {
        // Arrange
        var context = await GetDbContextAsync();
        var repository = new ProductRepository(context);
        var product = await context.Products.FirstAsync();
        var originalStock = product.Stock;
        var newStock = originalStock + 10;
        
        // Act
        var result = await repository.UpdateStockAsync(product.Id, newStock);
        
        // Assert
        Assert.True(result);
        var updatedProduct = await context.Products.FindAsync(product.Id);
        Assert.Equal(newStock, updatedProduct!.Stock);
    }
    
    [Fact]
    public async Task UpdateStockAsync_WithInvalidProduct_ShouldReturnFalse()
    {
        // Arrange
        var context = await GetDbContextAsync();
        var repository = new ProductRepository(context);
        
        // Act
        var result = await repository.UpdateStockAsync(Guid.NewGuid(), 10);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public async Task DecreaseStockAsync_WithSufficientStock_ShouldDecrease()
    {
        // Arrange
        var context = await GetDbContextAsync();
        var repository = new ProductRepository(context);
        var product = await context.Products.FirstAsync(p => p.Stock >= 10);
        var originalStock = product.Stock;
        var quantity = 5;
        
        // Act
        var result = await repository.DecreaseStockAsync(product.Id, quantity);
        
        // Assert
        Assert.True(result);
        var updatedProduct = await context.Products.FindAsync(product.Id);
        Assert.Equal(originalStock - quantity, updatedProduct!.Stock);
    }
    
    [Fact]
    public async Task DecreaseStockAsync_WithInsufficientStock_ShouldReturnFalse()
    {
        // Arrange
        var context = await GetDbContextAsync();
        var repository = new ProductRepository(context);
        var product = await context.Products.FirstAsync(p => p.Stock == 3);
        
        // Act
        var result = await repository.DecreaseStockAsync(product.Id, 10);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public async Task SearchProductsAsync_WithSearchTerm_ShouldReturnMatchingProducts()
    {
        // Arrange
        var context = await GetDbContextAsync();
        var repository = new ProductRepository(context);
        
        // Act
        var results = await repository.SearchProductsAsync("Laptop");
        
        // Assert
        Assert.Contains(results, p => p.Name.Contains("Laptop"));
    }
    
    [Fact]
    public async Task GetStockForMultipleProductsAsync_ShouldReturnDictionary()
    {
        // Arrange
        var context = await GetDbContextAsync();
        var repository = new ProductRepository(context);
        var products = await context.Products.Take(2).ToListAsync();
        var productIds = products.Select(p => p.Id);
        
        // Act
        var stockDict = await repository.GetStockForMultipleProductsAsync(productIds);
        
        // Assert
        Assert.Equal(2, stockDict.Count);
        foreach (var product in products)
        {
            Assert.Contains(product.Id, stockDict);
            Assert.Equal(product.Stock, stockDict[product.Id]);
        }
    }
}