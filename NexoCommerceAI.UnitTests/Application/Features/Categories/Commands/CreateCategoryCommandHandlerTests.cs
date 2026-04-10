using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Categories.Commands;
using NexoCommerceAI.Application.Features.Categories.Handlers;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Categories.Commands;

public class CreateCategoryCommandHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly CreateCategoryCommandHandler _handler;

    public CreateCategoryCommandHandlerTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        Mock<ILogger<CreateCategoryCommandHandler>> loggerMock = new();
        
        _handler = new CreateCategoryCommandHandler(_categoryRepositoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateCategory_WhenValid()
    {
        // Arrange
        var command = new CreateCategoryCommand
        {
            Name = "Electronics",
            Slug = "electronics"
        };

        _categoryRepositoryMock.Setup(r => r.IsSlugUniqueAsync(command.Slug, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(command.Name, result.Name);
        Assert.Equal(command.Slug, result.Slug);
        Assert.True(result.IsActive);
        
        _categoryRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
        _categoryRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldAutoGenerateSlug_WhenNotProvided()
    {
        // Arrange
        var command = new CreateCategoryCommand
        {
            Name = "Electronics & Gadgets",
            Slug = null
        };

        _categoryRepositoryMock.Setup(r => r.IsSlugUniqueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(command.Name, result.Name);
        Assert.Equal("electronics-and-gadgets", result.Slug);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenSlugAlreadyExists()
    {
        // Arrange
        var command = new CreateCategoryCommand
        {
            Name = "Electronics",
            Slug = "existing-slug"
        };

        _categoryRepositoryMock.Setup(r => r.IsSlugUniqueAsync(command.Slug, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));
        _categoryRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}