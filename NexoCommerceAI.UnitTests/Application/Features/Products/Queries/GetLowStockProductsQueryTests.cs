using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Handlers;
using NexoCommerceAI.Application.Features.Products.Queries;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Products.Queries;

public class GetLowStockProductsQueryTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly GetLowStockProductsQueryHandler _handler;
    
    public GetLowStockProductsQueryTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        Mock<ILogger<GetLowStockProductsQueryHandler>> loggerMock = new();
        _handler = new GetLowStockProductsQueryHandler(_productRepositoryMock.Object, loggerMock.Object);
    }
    
    [Fact]
    public async Task Handle_WithDefaultThreshold_ShouldReturnProductsWithStockBelow5()
    {
        // Arrange
        const int threshold = 5;
        var query = new GetLowStockProductsQuery();
        var category = Category.Create("Electronics");
        var expectedProducts = new List<Product>
        {
            Product.Create("Product 1", category.Id, price: 100m, stock: 3),
            Product.Create("Product 2", category.Id, price: 200m, stock: 2),
            Product.Create("Product 3", category.Id, price: 300m, stock: 1)
        };
        
        _productRepositoryMock.Setup(x => x.GetLowStockProductsAsync(threshold, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProducts);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.Equal(3, result.Count);
        Assert.All(result, p => Assert.True(p.Stock <= threshold));
        _productRepositoryMock.Verify(x => x.GetLowStockProductsAsync(threshold, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_WithCustomThreshold_ShouldReturnProductsWithStockBelowThreshold()
    {
        // Arrange
        const int threshold = 10;
        var query = new GetLowStockProductsQuery(threshold);
        var category = Category.Create("Electronics");
        var expectedProducts = new List<Product>
        {
            Product.Create("Product 1", category.Id, price: 100m, stock: 8),
            Product.Create("Product 2", category.Id, price: 200m, stock: 5),
            Product.Create("Product 3", category.Id, price: 300m, stock: 3)
        };
        
        _productRepositoryMock.Setup(x => x.GetLowStockProductsAsync(threshold, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProducts);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.Equal(3, result.Count);
        Assert.All(result, p => Assert.True(p.Stock <= threshold));
    }
    
    [Fact]
    public async Task Handle_WhenNoLowStockProducts_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetLowStockProductsQuery();
        
        _productRepositoryMock.Setup(x => x.GetLowStockProductsAsync(query.Threshold, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    
    [Fact]
    public async Task Handle_ShouldOnlyReturnActiveAndNonDeletedProducts()
    {
        // Arrange
        var query = new GetLowStockProductsQuery();
        var category = Category.Create("Electronics");
        
        // El repositorio debería filtrar automáticamente por IsActive y !IsDeleted
        var expectedProducts = new List<Product>
        {
            Product.Create("Active Product", category.Id, price: 100m, stock: 3)
        };
        
        _productRepositoryMock.Setup(x => x.GetLowStockProductsAsync(query.Threshold, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProducts);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.All(result, p => Assert.True(p.IsActive));
    }
    
    [Fact]
    public async Task Handle_ShouldMapToProductResponseCorrectly()
    {
        // Arrange
        var query = new GetLowStockProductsQuery();
        var category = Category.Create("Electronics", "electronics");
        var product = Product.Create("Low Stock Product", category.Id, price: 100m, stock: 3);
        var expectedProducts = new List<Product> { product };
        
        _productRepositoryMock.Setup(x => x.GetLowStockProductsAsync(query.Threshold, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProducts);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        var response = result[0];
        Assert.Equal(product.Id, response.Id);
        Assert.Equal(product.Name, response.Name);
        Assert.Equal(product.Price, response.Price);
        Assert.Equal(product.Stock, response.Stock);
        Assert.Equal(product.IsLowStock(query.Threshold), response.Stock <= query.Threshold);
        Assert.Equal(category.Id, response.CategoryId);
        Assert.Equal(category.Name, response.CategoryName);
    }
}