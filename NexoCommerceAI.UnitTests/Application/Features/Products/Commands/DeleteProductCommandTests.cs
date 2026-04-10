using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Commands;
using NexoCommerceAI.Application.Features.Products.Handlers;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Products.Commands;

public class DeleteProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly DeleteProductCommandHandler _handler;

    public DeleteProductCommandHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        Mock<ILogger<DeleteProductCommandHandler>> loggerMock = new();
        
        _handler = new DeleteProductCommandHandler(_productRepositoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldSoftDeleteProduct_WhenValid()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new DeleteProductCommand(productId);
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", categoryId, price: 100m, stock: 0);

        _productRepositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.True(product.IsDeleted);
        Assert.False(product.IsActive);
        _productRepositoryMock.Verify(r => r.UpdateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
        _productRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenProductNotFound()
    {
        // Arrange
        var command = new DeleteProductCommand(Guid.NewGuid());

        _productRepositoryMock.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenProductAlreadyDeleted()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new DeleteProductCommand(productId);
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", categoryId, price: 100m, stock: 0);
        product.SoftDelete();

        _productRepositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
        _productRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenProductHasPositiveStock()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new DeleteProductCommand(productId);
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", categoryId, price: 100m, stock: 10);

        _productRepositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));
        _productRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}