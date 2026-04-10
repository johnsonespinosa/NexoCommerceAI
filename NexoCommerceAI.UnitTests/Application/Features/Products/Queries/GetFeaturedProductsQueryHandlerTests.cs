using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Handlers;
using NexoCommerceAI.Application.Features.Products.Queries;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Products.Queries;

public class GetFeaturedProductsQueryHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly GetFeaturedProductsQueryHandler _handler;

    public GetFeaturedProductsQueryHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        Mock<ILogger<GetFeaturedProductsQueryHandler>> loggerMock = new();
        
        _handler = new GetFeaturedProductsQueryHandler(_productRepositoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFeaturedProducts_WhenExist()
    {
        // Arrange
        var take = 5;
        var query = new GetFeaturedProductsQuery(take);
        var category = Category.Create("Electronics");
        var products = new List<Product>
        {
            Product.Create("Featured 1", category.Id, price: 100m, stock: 10, isFeatured: true),
            Product.Create("Featured 2", category.Id, price: 200m, stock: 20, isFeatured: true)
        };

        _productRepositoryMock.Setup(r => r.GetFeaturedProductsAsync(take, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.True(p.IsFeatured));
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoFeaturedProducts()
    {
        // Arrange
        var query = new GetFeaturedProductsQuery(5);

        _productRepositoryMock.Setup(r => r.GetFeaturedProductsAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}