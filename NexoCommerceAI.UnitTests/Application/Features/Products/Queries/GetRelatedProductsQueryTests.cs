using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Handlers;
using NexoCommerceAI.Application.Features.Products.Queries;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Products.Queries;

public class GetRelatedProductsQueryTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly GetRelatedProductsQueryHandler _handler;
    
    public GetRelatedProductsQueryTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        Mock<ILogger<GetRelatedProductsQueryHandler>> loggerMock = new();
        _handler = new GetRelatedProductsQueryHandler(_productRepositoryMock.Object, loggerMock.Object);
    }
    
    [Fact]
    public async Task Handle_WithValidProduct_ShouldReturnRelatedProducts()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create("Electronics", "electronics");
        var productId = Guid.NewGuid();
        var product = Product.Create("Main Product", categoryId, price: 100m, stock: 10);
        var relatedProducts = new List<Product>
        {
            Product.Create("Related 1", categoryId, price: 80m, stock: 5),
            Product.Create("Related 2", categoryId, price: 120m, stock: 8),
            Product.Create("Related 3", categoryId, price: 90m, stock: 3)
        };
        
        var query = new GetRelatedProductsQuery(productId);
        
        _productRepositoryMock.Setup(x => x.GetByIdWithCategoryAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        
        _productRepositoryMock.Setup(x => x.GetProductsByCategoryAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(relatedProducts);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.Equal(3, result.Count);
        Assert.All(result, p => Assert.NotEqual(productId, p.Id));
    }
    
    [Fact]
    public async Task Handle_WithNoRelatedProducts_ShouldReturnEmptyList()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var product = Product.Create("Main Product", categoryId, price: 100m, stock: 10);
        var query = new GetRelatedProductsQuery(productId);
        
        _productRepositoryMock.Setup(x => x.GetByIdWithCategoryAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        
        _productRepositoryMock.Setup(x => x.GetProductsByCategoryAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    
    [Fact]
    public async Task Handle_WithNonExistingProduct_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetRelatedProductsQuery(Guid.NewGuid());
        
        _productRepositoryMock.Setup(x => x.GetByIdWithCategoryAsync(query.ProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    
    [Fact]
    public async Task Handle_ShouldExcludeCurrentProduct()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var product = Product.Create("Main Product", categoryId, price: 100m, stock: 10);
        var relatedProducts = new List<Product>
        {
            product, // El mismo producto
            Product.Create("Related 1", categoryId, price: 80m, stock: 5)
        };
        
        var query = new GetRelatedProductsQuery(productId);
        
        _productRepositoryMock.Setup(x => x.GetByIdWithCategoryAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        
        _productRepositoryMock.Setup(x => x.GetProductsByCategoryAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(relatedProducts);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.DoesNotContain(result, p => p.Id == productId);
    }
    
    [Fact]
    public async Task Handle_ShouldLimitResultsByTake()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var take = 2;
        var product = Product.Create("Main Product", categoryId, price: 100m, stock: 10);
        var relatedProducts = new List<Product>
        {
            Product.Create("Related 1", categoryId, price: 80m, stock: 5),
            Product.Create("Related 2", categoryId, price: 120m, stock: 8),
            Product.Create("Related 3", categoryId, price: 90m, stock: 3)
        };
        
        var query = new GetRelatedProductsQuery(productId, take);
        
        _productRepositoryMock.Setup(x => x.GetByIdWithCategoryAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        
        _productRepositoryMock.Setup(x => x.GetProductsByCategoryAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(relatedProducts);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.Equal(take, result.Count);
    }
}