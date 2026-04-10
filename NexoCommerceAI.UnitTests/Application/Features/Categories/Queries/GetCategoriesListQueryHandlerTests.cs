using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Common.Models;
using NexoCommerceAI.Application.Features.Categories.Handlers;
using NexoCommerceAI.Application.Features.Categories.Queries;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Categories.Queries;

public class GetCategoriesListQueryHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly GetCategoriesListQueryHandler _handler;

    public GetCategoriesListQueryHandlerTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        Mock<ILogger<GetCategoriesListQueryHandler>> loggerMock = new();
        
        _handler = new GetCategoriesListQueryHandler(_categoryRepositoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedResult_WhenCategoriesExist()
    {
        // Arrange
        var pagination = new PaginationParams { PageNumber = 1, PageSize = 10 };
        var query = new GetCategoriesListQuery { Pagination = pagination };
        var categories = new List<Category>
        {
            Category.Create("Electronics", "electronics"),
            Category.Create("Books", "books")
        };

        _categoryRepositoryMock.Setup(r => r.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task Handle_ShouldFilterCategories_WhenSearchTermProvided()
    {
        // Arrange
        var pagination = new PaginationParams 
        { 
            PageNumber = 1, 
            PageSize = 10,
            SearchTerm = "Electronics"
        };
        var query = new GetCategoriesListQuery { Pagination = pagination };
        var categories = new List<Category>
        {
            Category.Create("Electronics", "electronics"),
            Category.Create("Books", "books"),
            Category.Create("Home & Garden", "home-garden")
        };

        _categoryRepositoryMock.Setup(r => r.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal("Electronics", result.Items.First().Name);
    }
}