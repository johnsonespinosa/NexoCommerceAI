using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Handlers;
using NexoCommerceAI.Application.Features.Products.Queries;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Products.Queries;

public class GetProductBySlugQueryTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly GetProductBySlugQueryHandler _handler;
    
    public GetProductBySlugQueryTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        Mock<ILogger<GetProductBySlugQueryHandler>> loggerMock = new();
        _handler = new GetProductBySlugQueryHandler(_productRepositoryMock.Object, loggerMock.Object);
    }
    
    [Fact]
    public async Task Handle_WithExistingSlug_ShouldReturnProductResponse()
    {
        // Arrange
        var slug = "test-product";
        var category = Category.Create("Electronics");
        var product = Product.Create("Test Product", category.Id, slug: slug, price: 100m, stock: 10);
        var query = new GetProductBySlugQuery(slug);
        
        _productRepositoryMock.Setup(x => x.GetBySlugAsync(slug, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(product.Id, result.Id);
        Assert.Equal(product.Name, result.Name);
        Assert.Equal(product.Slug, result.Slug);
    }
    
    [Fact]
    public async Task Handle_WithNonExistingSlug_ShouldReturnNull()
    {
        // Arrange
        var slug = "non-existing-slug";
        var query = new GetProductBySlugQuery(slug);
        
        _productRepositoryMock.Setup(x => x.GetBySlugAsync(slug, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task Handle_ShouldIncludeCategoryInformation()
    {
        // Arrange
        var slug = "test-product";
        var category = Category.Create("Electronics", "electronics");
        var product = Product.Create("Test Product", category.Id, slug: slug, price: 100m, stock: 10);
        var query = new GetProductBySlugQuery(slug);
        
        _productRepositoryMock.Setup(x => x.GetBySlugAsync(slug, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(category.Id, result.CategoryId);
        Assert.Equal(category.Name, result.CategoryName);
        Assert.Equal(category.Slug, result.CategorySlug);
    }
}
