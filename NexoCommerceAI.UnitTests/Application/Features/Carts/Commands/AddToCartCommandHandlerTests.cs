using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Carts.Commands;
using NexoCommerceAI.Application.Features.Carts.Handlers;
using NexoCommerceAI.Application.Features.Carts.Models;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Carts.Commands;

public class AddToCartCommandHandlerTests
{
    private readonly Mock<ICartRepository> _cartRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ICartCacheService> _cartCacheServiceMock;
    private readonly AddToCartCommandHandler _handler;

    public AddToCartCommandHandlerTests()
    {
        _cartRepositoryMock = new Mock<ICartRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _cartCacheServiceMock = new Mock<ICartCacheService>();
        Mock<ILogger<AddToCartCommandHandler>> loggerMock = new();
        
        _handler = new AddToCartCommandHandler(
            _cartRepositoryMock.Object,
            _productRepositoryMock.Object,
            _cartCacheServiceMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldAddItemToExistingCart_WhenCartExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", categoryId, price: 100m, stock: 10);
        var cart = Cart.Create(userId);
        var command = new AddToCartCommand(userId, productId, 2);

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _cartRepositoryMock.Setup(x => x.GetByUserIdWithItemsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(2, result.Items.First().Quantity);
        Assert.Equal(product.Price * 2, result.TotalAmount);
        _cartRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Cart>(), It.IsAny<CancellationToken>()), Times.Once);
        _cartRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cartCacheServiceMock.Verify(x => x.SetCartAsync(userId, It.IsAny<CartResponse>(), null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateNewCart_WhenCartDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", categoryId, price: 100m, stock: 10);
        var command = new AddToCartCommand(userId, productId, 2);

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _cartRepositoryMock.Setup(x => x.GetByUserIdWithItemsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cart?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        _cartRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Cart>(), It.IsAny<CancellationToken>()), Times.Once);
        _cartRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Cart>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenProductNotFound()
    {
        // Arrange
        var command = new AddToCartCommand(Guid.NewGuid(), Guid.NewGuid(), 1);

        _productRepositoryMock.Setup(x => x.GetByIdAsync(command.ProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenProductNotAvailable()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", categoryId, price: 100m, stock: 10);
        product.Deactivate();
        var command = new AddToCartCommand(Guid.NewGuid(), productId, 1);

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenInsufficientStock()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", categoryId, price: 100m, stock: 3);
        var command = new AddToCartCommand(Guid.NewGuid(), productId, 5);

        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));
    }
}