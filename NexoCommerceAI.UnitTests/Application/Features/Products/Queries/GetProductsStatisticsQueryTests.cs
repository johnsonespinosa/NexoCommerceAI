using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Handlers;
using NexoCommerceAI.Application.Features.Products.Queries;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Products.Queries;

public class GetProductsStatisticsQueryTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly GetProductsStatisticsQueryHandler _handler;
    
    public GetProductsStatisticsQueryTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        Mock<ILogger<GetProductsStatisticsQueryHandler>> loggerMock = new();
        _handler = new GetProductsStatisticsQueryHandler(
            _productRepositoryMock.Object, 
            _categoryRepositoryMock.Object, 
            loggerMock.Object);
    }
    
    [Fact]
    public async Task Handle_ShouldCalculateCorrectStatistics()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create("Electronics", "electronics");
        var products = new List<Product>
        {
            Product.Create("Product 1", categoryId, price: 100m, stock: 10, isFeatured: true),
            Product.Create("Product 2", categoryId, price: 200m, stock: 5, isFeatured: false),
            Product.Create("Product 3", categoryId, price: 50m, stock: 0, isFeatured: false)
        };
        
        _productRepositoryMock.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);
        
        _categoryRepositoryMock.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category> { category });
        
        // Act
        var result = await _handler.Handle(new GetProductsStatisticsQuery(), CancellationToken.None);
        
        // Assert
        Assert.Equal(3, result.TotalProducts);
        Assert.Equal(3, result.ActiveProducts); // Todos activos
        Assert.Equal(0, result.InactiveProducts);
        Assert.Equal(15, result.TotalStockUnits); // 10 + 5 + 0
        Assert.Equal(100*10 + 200*5, result.TotalStockValue); // 1000 + 1000 = 2000
        Assert.Equal(1, result.FeaturedProducts);
    }
    
    [Fact]
    public async Task Handle_ShouldCountInactiveAndDeletedProducts()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var activeProduct = Product.Create("Active", categoryId, price: 100m, stock: 10);
        var inactiveProduct = Product.Create("Inactive", categoryId, price: 100m, stock: 5);
        inactiveProduct.Deactivate();
        var deletedProduct = Product.Create("Deleted", categoryId, price: 100m, stock: 3);
        deletedProduct.SoftDelete();
        
        var products = new List<Product> { activeProduct, inactiveProduct, deletedProduct };
        
        _productRepositoryMock.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);
        
        _categoryRepositoryMock.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>());
        
        // Act
        var result = await _handler.Handle(new GetProductsStatisticsQuery(), CancellationToken.None);
        
        // Assert
        Assert.Equal(3, result.TotalProducts);
        Assert.Equal(1, result.ActiveProducts);
        Assert.Equal(1, result.InactiveProducts);
        Assert.Equal(1, result.DeletedProducts);
        Assert.Equal(10, result.TotalStockUnits); // Solo productos activos
    }
    
    [Fact]
    public async Task Handle_ShouldCalculatePriceRanges()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var products = new List<Product>
        {
            Product.Create("Product 1", categoryId, price: 50m, stock: 10),
            Product.Create("Product 2", categoryId, price: 100m, stock: 5),
            Product.Create("Product 3", categoryId, price: 200m, stock: 3)
        };
        
        _productRepositoryMock.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);
        
        _categoryRepositoryMock.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>());
        
        // Act
        var result = await _handler.Handle(new GetProductsStatisticsQuery(), CancellationToken.None);
        
        // Assert
        Assert.Equal(50m, result.MinPrice);
        Assert.Equal(200m, result.MaxPrice);
        Assert.Equal(116.67m, Math.Round(result.AveragePrice, 2));
    }
    
    [Fact]
    public async Task Handle_ShouldCountProductsOnSale()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var products = new List<Product>
        {
            Product.Create("Product 1", categoryId, price: 80m, compareAtPrice: 100m, stock: 10), // On sale
            Product.Create("Product 2", categoryId, price: 100m, stock: 5), // Not on sale
            Product.Create("Product 3", categoryId, price: 90m, compareAtPrice: 120m, stock: 3) // On sale
        };
        
        _productRepositoryMock.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);
        
        _categoryRepositoryMock.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>());
        
        // Act
        var result = await _handler.Handle(new GetProductsStatisticsQuery(), CancellationToken.None);
        
        // Assert
        Assert.Equal(2, result.ProductsOnSale);
    }
    
    [Fact]
    public async Task Handle_ShouldCountLowStockAndOutOfStock()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var products = new List<Product>
        {
            Product.Create("Product 1", categoryId, price: 100m, stock: 3),  // Low stock
            Product.Create("Product 2", categoryId, price: 100m, stock: 0),  // Out of stock
            Product.Create("Product 3", categoryId, price: 100m, stock: 10)  // Normal stock
        };
        
        _productRepositoryMock.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);
        
        _categoryRepositoryMock.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>());
        
        // Act
        var result = await _handler.Handle(new GetProductsStatisticsQuery(), CancellationToken.None);
        
        // Assert
        Assert.Equal(1, result.LowStockProducts);
        Assert.Equal(1, result.OutOfStockProducts);
    }
    
    [Fact]
    public async Task Handle_WithNoProducts_ShouldReturnZeroStatistics()
    {
        // Arrange
        _productRepositoryMock.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());
        
        _categoryRepositoryMock.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>());
        
        // Act
        var result = await _handler.Handle(new GetProductsStatisticsQuery(), CancellationToken.None);
        
        // Assert
        Assert.Equal(0, result.TotalProducts);
        Assert.Equal(0, result.TotalStockUnits);
        Assert.Equal(0, result.TotalStockValue);
        Assert.Equal(0, result.MinPrice);
        Assert.Equal(0, result.MaxPrice);
        Assert.Equal(0, result.AveragePrice);
    }
}