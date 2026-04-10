using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Categories.Handlers;
using NexoCommerceAI.Application.Features.Categories.Queries;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Categories.Queries;

public class GetCategoryBySlugQueryHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly GetCategoryBySlugQueryHandler _handler;

    public GetCategoryBySlugQueryHandlerTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        Mock<ILogger<GetCategoryBySlugQueryHandler>> loggerMock = new();
        
        _handler = new GetCategoryBySlugQueryHandler(_categoryRepositoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnCategory_WhenExists()
    {
        // Arrange
        var slug = "electronics";
        var query = new GetCategoryBySlugQuery(slug);
        var category = Category.Create("Electronics", slug);

        _categoryRepositoryMock.Setup(r => r.GetBySlugAsync(slug, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(category.Id, result.Id);
        Assert.Equal(category.Name, result.Name);
        Assert.Equal(category.Slug, result.Slug);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenCategoryNotFound()
    {
        // Arrange
        var query = new GetCategoryBySlugQuery("nonexistent");

        _categoryRepositoryMock.Setup(r => r.GetBySlugAsync(query.Slug, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }
}