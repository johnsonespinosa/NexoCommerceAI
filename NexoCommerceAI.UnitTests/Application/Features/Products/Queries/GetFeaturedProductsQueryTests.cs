using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Handlers;
using NexoCommerceAI.Application.Features.Products.Queries;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Products.Queries;

public class GetFeaturedProductsQueryTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ILogger<GetFeaturedProductsQueryHandler>> _loggerMock;
    private readonly GetFeaturedProductsQueryHandler _handler;
    
    public GetFeaturedProductsQueryTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _loggerMock = new Mock<ILogger<GetFeaturedProductsQueryHandler>>();
        _handler = new GetFeaturedProductsQueryHandler(_productRepositoryMock.Object, _loggerMock.Object);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnFeaturedProducts()
    {
        // Arrange
        var take = 5;
        var query = new GetFeaturedProductsQuery(take);
        var category = Category.Create("Electronics");
        var expectedProducts = new List<Product>
        {
            Product.Create("Product 1", category.Id, price: 100m, stock: 10, isFeatured: true),
            Product.Create("Product 2", category.Id, price: 200m, stock: 20, isFeatured: true)
        };
        
        _productRepositoryMock.Setup(x => x.GetFeaturedProductsAsync(take, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProducts);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedProducts.Count, result.Count);
        _productRepositoryMock.Verify(x => x.GetFeaturedProductsAsync(take, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_WithNoFeaturedProducts_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetFeaturedProductsQuery(10);
        
        _productRepositoryMock.Setup(x => x.GetFeaturedProductsAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    
    [Fact]
    public async Task Handle_WithTakeLessThanAvailable_ShouldReturnLimitedResults()
    {
        // Arrange
        var take = 2;
        var query = new GetFeaturedProductsQuery(take);
        var category = Category.Create("Electronics");
        var expectedProducts = new List<Product>
        {
            Product.Create("Product 1", category.Id, price: 100m, stock: 10, isFeatured: true),
            Product.Create("Product 2", category.Id, price: 200m, stock: 20, isFeatured: true)
        };
        
        _productRepositoryMock.Setup(x => x.GetFeaturedProductsAsync(take, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProducts);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.Equal(take, result.Count);
    }
}