using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Common.Models;
using NexoCommerceAI.Application.Features.Products.Handlers;
using NexoCommerceAI.Application.Features.Products.Queries;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Products.Queries;

public class GetProductsListQueryHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly GetProductsListQueryHandler _handler;

    public GetProductsListQueryHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        Mock<ILogger<GetProductsListQueryHandler>> loggerMock = new();
        
        _handler = new GetProductsListQueryHandler(_productRepositoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedResult_WhenProductsExist()
    {
        // Arrange
        var pagination = new PaginationParams { PageNumber = 1, PageSize = 10 };
        var query = new GetProductsListQuery { Pagination = pagination };
        var category = Category.Create("Electronics");
        var products = new List<Product>
        {
            Product.Create("Product 1", category.Id, price: 100m, stock: 10),
            Product.Create("Product 2", category.Id, price: 200m, stock: 20)
        };

        _productRepositoryMock.Setup(r => r.GetPaginatedProductsAsync(
            pagination.PageNumber, pagination.PageSize, pagination.SearchTerm, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyResult_WhenNoProducts()
    {
        // Arrange
        var pagination = new PaginationParams { PageNumber = 1, PageSize = 10 };
        var query = new GetProductsListQuery { Pagination = pagination };

        _productRepositoryMock.Setup(r => r.GetPaginatedProductsAsync(
            pagination.PageNumber, pagination.PageSize, pagination.SearchTerm, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Items);
    }
}