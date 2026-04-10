using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Handlers;
using NexoCommerceAI.Application.Features.Products.Queries;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Products.Queries;

public class SearchProductsQueryTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly SearchProductsQueryHandler _handler;
    
    public SearchProductsQueryTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        Mock<ILogger<SearchProductsQueryHandler>> loggerMock = new();
        _handler = new SearchProductsQueryHandler(_productRepositoryMock.Object, loggerMock.Object);
    }
    
    [Fact]
    public async Task Handle_WithValidSearchTerm_ShouldReturnMatchingProducts()
    {
        // Arrange
        var searchTerm = "laptop";
        var take = 10;
        var query = new SearchProductsQuery(searchTerm, take);
        var category = Category.Create("Electronics");
        var expectedProducts = new List<Product>
        {
            Product.Create("Gaming Laptop", category.Id, price: 1000m, stock: 10),
            Product.Create("Business Laptop", category.Id, price: 800m, stock: 5)
        };
        
        _productRepositoryMock.Setup(x => x.SearchProductsAsync(searchTerm, take, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProducts);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Name.Contains("Laptop"));
        _productRepositoryMock.Verify(x => x.SearchProductsAsync(searchTerm, take, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_WithNoMatchingProducts_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new SearchProductsQuery("nonexistent", 10);
        
        _productRepositoryMock.Setup(x => x.SearchProductsAsync(query.SearchTerm, query.Take, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    
    [Fact]
    public async Task Handle_ShouldLimitResultsByTake()
    {
        // Arrange
        var take = 2;
        var query = new SearchProductsQuery("product", take);
        var category = Category.Create("Electronics");
        var allProducts = new List<Product>
        {
            Product.Create("Product 1", category.Id, price: 100m, stock: 10),
            Product.Create("Product 2", category.Id, price: 200m, stock: 20),
            Product.Create("Product 3", category.Id, price: 300m, stock: 30)
        };
        
        _productRepositoryMock.Setup(x => x.SearchProductsAsync(query.SearchTerm, take, It.IsAny<CancellationToken>()))
            .ReturnsAsync(allProducts.Take(take).ToList());
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.Equal(take, result.Count);
    }
}