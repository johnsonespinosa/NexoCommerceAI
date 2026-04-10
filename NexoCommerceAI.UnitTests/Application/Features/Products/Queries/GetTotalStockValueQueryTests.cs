using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Handlers;
using NexoCommerceAI.Application.Features.Products.Queries;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Products.Queries;

public class GetTotalStockValueQueryTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly GetTotalStockValueQueryHandler _handler;
    
    public GetTotalStockValueQueryTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        Mock<ILogger<GetTotalStockValueQueryHandler>> loggerMock = new();
        _handler = new GetTotalStockValueQueryHandler(
            _productRepositoryMock.Object, 
            _categoryRepositoryMock.Object, 
            loggerMock.Object);
    }
    
    [Fact]
    public async Task Handle_ShouldCalculateCorrectTotalValues()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var products = new List<Product>
        {
            Product.Create("Product 1", categoryId, price: 100m, stock: 10),
            Product.Create("Product 2", categoryId, price: 200m, stock: 5),
            Product.Create("Product 3", categoryId, price: 50m, stock: 20)
        };
        
        _productRepositoryMock.Setup(x => x.GetActiveProductsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);
        
        _categoryRepositoryMock.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>());
        
        // Act
        var result = await _handler.Handle(new GetTotalStockValueQuery(), CancellationToken.None);
        
        // Assert
        Assert.Equal(35, result.TotalUnits); // 10 + 5 + 20
        Assert.Equal(100*10 + 200*5 + 50*20, result.TotalValue); // 1000 + 1000 + 1000 = 3000
        Assert.Equal(3, result.ActiveProductsCount);
    }
    
    [Fact]
    public async Task Handle_WithEmptyProductList_ShouldReturnZeroValues()
    {
        // Arrange
        _productRepositoryMock.Setup(x => x.GetActiveProductsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());
        
        _categoryRepositoryMock.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>());
        
        // Act
        var result = await _handler.Handle(new GetTotalStockValueQuery(), CancellationToken.None);
        
        // Assert
        Assert.Equal(0, result.TotalUnits);
        Assert.Equal(0, result.TotalValue);
        Assert.Equal(0, result.AveragePrice);
        Assert.Equal(0, result.ActiveProductsCount);
        Assert.Equal(0, result.LowStockProductsCount);
        Assert.Equal(0, result.OutOfStockProductsCount);
    }
    
    [Fact]
    public async Task Handle_ShouldCountLowStockProducts()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var products = new List<Product>
        {
            Product.Create("Product 1", categoryId, price: 100m, stock: 3),  // Low stock (<=5)
            Product.Create("Product 2", categoryId, price: 200m, stock: 8),  // Normal stock
            Product.Create("Product 3", categoryId, price: 50m, stock: 2)    // Low stock
        };
        
        _productRepositoryMock.Setup(x => x.GetActiveProductsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);
        
        _categoryRepositoryMock.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>());
        
        // Act
        var result = await _handler.Handle(new GetTotalStockValueQuery(), CancellationToken.None);
        
        // Assert
        Assert.Equal(2, result.LowStockProductsCount);
    }
    
    [Fact]
    public async Task Handle_ShouldCountOutOfStockProducts()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var products = new List<Product>
        {
            Product.Create("Product 1", categoryId, price: 100m, stock: 0),  // Out of stock
            Product.Create("Product 2", categoryId, price: 200m, stock: 5),  // In stock
            Product.Create("Product 3", categoryId, price: 50m, stock: 0)    // Out of stock
        };
        
        _productRepositoryMock.Setup(x => x.GetActiveProductsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);
        
        _categoryRepositoryMock.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>());
        
        // Act
        var result = await _handler.Handle(new GetTotalStockValueQuery(), CancellationToken.None);
        
        // Assert
        Assert.Equal(2, result.OutOfStockProductsCount);
    }
    
    [Fact]
    public async Task Handle_ShouldCalculateAveragePrice()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var products = new List<Product>
        {
            Product.Create("Product 1", categoryId, price: 100m, stock: 10),
            Product.Create("Product 2", categoryId, price: 200m, stock: 5),
            Product.Create("Product 3", categoryId, price: 300m, stock: 20)
        };
        
        _productRepositoryMock.Setup(x => x.GetActiveProductsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);
        
        _categoryRepositoryMock.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>());
        
        // Act
        var result = await _handler.Handle(new GetTotalStockValueQuery(), CancellationToken.None);
        
        // Assert
        Assert.Equal(200m, result.AveragePrice); // (100 + 200 + 300) / 3 = 200
    }
}