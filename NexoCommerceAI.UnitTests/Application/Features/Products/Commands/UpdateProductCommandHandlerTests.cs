using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Commands;
using NexoCommerceAI.Application.Features.Products.Handlers;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Products.Commands;

public class UpdateProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly UpdateProductCommandHandler _handler;

    public UpdateProductCommandHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        Mock<ILogger<UpdateProductCommandHandler>> loggerMock = new();
        
        _handler = new UpdateProductCommandHandler(
            _productRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateProduct_WhenValid()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var command = new UpdateProductCommand
        (
            Id: productId,
            Name: "Updated Product",
            Price: 150m,
            Stock: 20
        );
        
        Category.Create("Electronics");
        var product = Product.Create("Original Product", categoryId, price: 100m, stock: 10);

        _productRepositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Product", product.Name);
        Assert.Equal(150m, product.Price);
        Assert.Equal(20, product.Stock);
        _productRepositoryMock.Verify(r => r.UpdateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
        _productRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenProductNotFound()
    {
        // Arrange
        var command = new UpdateProductCommand ( Id: Guid.NewGuid(), Name: "Updated Product" );

        _productRepositoryMock.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenNewCategoryNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var newCategoryId = Guid.NewGuid();
        var command = new UpdateProductCommand(
            Id: productId,
            CategoryId: newCategoryId
            );
        
        var category = Category.Create("Electronics");
        var product = Product.Create("Test Product", category.Id, price: 100m, stock: 10);

        _productRepositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(newCategoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }
}