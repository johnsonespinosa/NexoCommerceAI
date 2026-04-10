using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Categories.Commands;
using NexoCommerceAI.Application.Features.Categories.Handlers;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Categories.Commands;

public class DeleteCategoryCommandHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly DeleteCategoryCommandHandler _handler;

    public DeleteCategoryCommandHandlerTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        Mock<ILogger<DeleteCategoryCommandHandler>> loggerMock = new();
        
        _handler = new DeleteCategoryCommandHandler(_categoryRepositoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldSoftDeleteCategory_WhenValid()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new DeleteCategoryCommand(categoryId);
        var category = Category.Create("Electronics", "electronics");

        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.True(category.IsDeleted);
        Assert.False(category.IsActive);
        _categoryRepositoryMock.Verify(r => r.UpdateAsync(category, It.IsAny<CancellationToken>()), Times.Once);
        _categoryRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenCategoryNotFound()
    {
        // Arrange
        var command = new DeleteCategoryCommand(Guid.NewGuid());

        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenCategoryHasProducts()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new DeleteCategoryCommand(categoryId);
        var category = Category.Create("Electronics", "electronics");
        
        // Agregar un producto a la categoría
        var product = Product.Create("Test Product", categoryId, price: 100m, stock: 10);
        category.AddProduct(product);

        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));
        _categoryRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}