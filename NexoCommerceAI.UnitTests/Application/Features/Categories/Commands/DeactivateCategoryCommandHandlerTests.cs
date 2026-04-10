using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Categories.Commands;
using NexoCommerceAI.Application.Features.Categories.Handlers;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Categories.Commands;

public class DeactivateCategoryCommandHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly DeactivateCategoryCommandHandler _handler;

    public DeactivateCategoryCommandHandlerTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        Mock<ILogger<DeactivateCategoryCommandHandler>> loggerMock = new();
        
        _handler = new DeactivateCategoryCommandHandler(_categoryRepositoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeactivateCategory_WhenActive()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new DeactivateCategoryCommand(categoryId);
        var category = Category.Create("Electronics", "electronics");

        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.False(category.IsActive);
        _categoryRepositoryMock.Verify(r => r.UpdateAsync(category, It.IsAny<CancellationToken>()), Times.Once);
        _categoryRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenAlreadyInactive()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new DeactivateCategoryCommand(categoryId);
        var category = Category.Create("Electronics", "electronics");
        category.Deactivate();

        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
        _categoryRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenCategoryNotFound()
    {
        // Arrange
        var command = new DeactivateCategoryCommand(Guid.NewGuid());

        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenCategoryIsDeleted()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new DeactivateCategoryCommand(categoryId);
        var category = Category.Create("Electronics", "electronics");
        category.SoftDelete();

        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));
    }
}