using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Categories.Commands;
using NexoCommerceAI.Application.Features.Categories.Handlers;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Categories.Commands;

public class UpdateCategoryCommandHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly UpdateCategoryCommandHandler _handler;

    public UpdateCategoryCommandHandlerTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        Mock<ILogger<UpdateCategoryCommandHandler>> loggerMock = new();
        
        _handler = new UpdateCategoryCommandHandler(_categoryRepositoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateCategory_WhenValid()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new UpdateCategoryCommand
        {
            Id = categoryId,
            Name = "Computers",
            Slug = "computers"
        };
        
        var category = Category.Create("Electronics", "electronics");

        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _categoryRepositoryMock.Setup(r => r.IsNameUniqueAsync(command.Name, categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _categoryRepositoryMock.Setup(r => r.IsSlugUniqueAsync(command.Slug, categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Computers", category.Name);
        Assert.Equal("computers", category.Slug);
        _categoryRepositoryMock.Verify(r => r.UpdateAsync(category, It.IsAny<CancellationToken>()), Times.Once);
        _categoryRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenCategoryNotFound()
    {
        // Arrange
        var command = new UpdateCategoryCommand { Id = Guid.NewGuid(), Name = "New Name" };

        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenNameAlreadyExists()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new UpdateCategoryCommand
        {
            Id = categoryId,
            Name = "ExistingName"
        };
        
        var category = Category.Create("Electronics", "electronics");

        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _categoryRepositoryMock.Setup(r => r.IsNameUniqueAsync(command.Name, categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));
    }
}