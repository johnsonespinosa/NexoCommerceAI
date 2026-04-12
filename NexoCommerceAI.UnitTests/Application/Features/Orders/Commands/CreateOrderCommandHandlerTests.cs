using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Orders.Commands;
using NexoCommerceAI.Application.Features.Orders.Handler;
using NexoCommerceAI.Application.Features.Orders.Models;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Orders.Commands;

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<ICartRepository> _cartRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _cartRepositoryMock = new Mock<ICartRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        Mock<ILogger<CreateOrderCommandHandler>> loggerMock = new();
        
        _handler = new CreateOrderCommandHandler(
            _orderRepositoryMock.Object,
            _cartRepositoryMock.Object,
            _productRepositoryMock.Object,
            loggerMock.Object);
    }

    private AddressDto CreateAddressDto() => new()
    {
        Street = "123 Main St",
        City = "New York",
        State = "NY",
        ZipCode = "10001",
        Country = "USA"
    };

    [Fact]
    public async Task Handle_ShouldCreateOrder_WhenCartHasItems()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", categoryId, price: 100m, stock: 10);
        var cart = Cart.Create(userId);
        cart.AddItem(product, 2);
        
        var command = new CreateOrderCommand(
            userId,
            CreateAddressDto(),
            CreateAddressDto());

        _cartRepositoryMock.Setup(x => x.GetByUserIdWithItemsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);
        _productRepositoryMock.Setup(x => x.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.StartsWith("ORD-", result.OrderNumber);
        Assert.Single(result.Items);
        Assert.Equal(200m, result.Subtotal);
        
        _orderRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _orderRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cartRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Cart>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenCartIsEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cart = Cart.Create(userId);
        
        var command = new CreateOrderCommand(
            userId,
            CreateAddressDto(),
            CreateAddressDto());

        _cartRepositoryMock.Setup(x => x.GetByUserIdWithItemsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenProductNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", categoryId, price: 100m, stock: 10);
        var cart = Cart.Create(userId);
        cart.AddItem(product, 2);
        
        var command = new CreateOrderCommand(
            userId,
            CreateAddressDto(),
            CreateAddressDto());

        _cartRepositoryMock.Setup(x => x.GetByUserIdWithItemsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);
        _productRepositoryMock.Setup(x => x.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }
}