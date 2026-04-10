using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Handlers;
using NexoCommerceAI.Application.Features.Products.Queries;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Products.Queries;

public class GetProductsByCategoryQueryTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly GetProductsByCategoryQueryHandler _handler;
    
    public GetProductsByCategoryQueryTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        Mock<ILogger<GetProductsByCategoryQueryHandler>> loggerMock = new();
        _handler = new GetProductsByCategoryQueryHandler(
            _productRepositoryMock.Object, 
            _categoryRepositoryMock.Object, 
            loggerMock.Object);
    }
    
    [Fact]
    public async Task Handle_WithValidCategory_ShouldReturnProducts()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create("Electronics", "electronics");
        var query = new GetProductsByCategoryQuery(categoryId);
        var expectedProducts = new List<Product>
        {
            Product.Create("Product 1", categoryId, price: 100m, stock: 10),
            Product.Create("Product 2", categoryId, price: 200m, stock: 20)
        };
        
        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        
        _productRepositoryMock.Setup(x => x.GetProductsByCategoryAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProducts);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.Equal(2, result.Count);
        _productRepositoryMock.Verify(x => x.GetProductsByCategoryAsync(categoryId, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_WithNonExistingCategory_ShouldReturnEmptyList()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var query = new GetProductsByCategoryQuery(categoryId);
        
        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _productRepositoryMock.Verify(x => x.GetProductsByCategoryAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_WithInactiveCategory_ShouldReturnEmptyList()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create("Electronics", "electronics");
        category.Deactivate(); // Marcar como inactiva
        var query = new GetProductsByCategoryQuery(categoryId);
        
        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    
    [Fact]
    public async Task Handle_WithDeletedCategory_ShouldReturnEmptyList()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create("Electronics", "electronics");
        category.SoftDelete(); // Marcar como eliminada
        var query = new GetProductsByCategoryQuery(categoryId);
        
        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    
    [Fact]
    public async Task Handle_WithTakeLimit_ShouldLimitResults()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var take = 2;
        var category = Category.Create("Electronics", "electronics");
        var query = new GetProductsByCategoryQuery(categoryId, take);
        var allProducts = new List<Product>
        {
            Product.Create("Product 1", categoryId, price: 100m, stock: 10),
            Product.Create("Product 2", categoryId, price: 200m, stock: 20),
            Product.Create("Product 3", categoryId, price: 300m, stock: 30)
        };
        
        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        
        _productRepositoryMock.Setup(x => x.GetProductsByCategoryAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(allProducts);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.Equal(take, result.Count);
    }
    
    [Fact]
    public async Task Handle_ShouldMapToProductResponseCorrectly()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create("Electronics", "electronics");
        var product = Product.Create("Test Product", categoryId, price: 100m, stock: 10);
        var query = new GetProductsByCategoryQuery(categoryId);
        var expectedProducts = new List<Product> { product };
        
        _categoryRepositoryMock.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        
        _productRepositoryMock.Setup(x => x.GetProductsByCategoryAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProducts);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        var response = result.First();
        Assert.Equal(product.Id, response.Id);
        Assert.Equal(product.Name, response.Name);
        Assert.Equal(product.Price, response.Price);
        Assert.Equal(product.Stock, response.Stock);
        Assert.Equal(category.Id, response.CategoryId);
        Assert.Equal(category.Name, response.CategoryName);
        Assert.Equal(category.Slug, response.CategorySlug);
    }
}