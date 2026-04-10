using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Categories.Handlers;
using NexoCommerceAI.Application.Features.Categories.Queries;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Categories.Queries;

public class GetCategoryByIdQueryHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly GetCategoryByIdQueryHandler _handler;

    public GetCategoryByIdQueryHandlerTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        Mock<ILogger<GetCategoryByIdQueryHandler>> loggerMock = new();
        
        _handler = new GetCategoryByIdQueryHandler(_categoryRepositoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnCategory_WhenExists()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var query = new GetCategoryByIdQuery(categoryId);
        var category = Category.Create("Electronics", "electronics");

        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(category.Id, result.Id);
        Assert.Equal(category.Name, result.Name);
        Assert.Equal(category.Slug, result.Slug);
        Assert.Equal(category.IsActive, result.IsActive);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenCategoryNotFound()
    {
        // Arrange
        var query = new GetCategoryByIdQuery(Guid.NewGuid());

        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(query.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }
}