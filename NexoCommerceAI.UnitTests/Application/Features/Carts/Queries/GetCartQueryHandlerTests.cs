using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Carts.Handlers;
using NexoCommerceAI.Application.Features.Carts.Models;
using NexoCommerceAI.Application.Features.Carts.Queries;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Carts.Queries;

public class GetCartQueryHandlerTests
{
    private readonly Mock<ICartRepository> _cartRepositoryMock;
    private readonly Mock<ICartCacheService> _cartCacheServiceMock;
    private readonly GetCartQueryHandler _handler;

    public GetCartQueryHandlerTests()
    {
        _cartRepositoryMock = new Mock<ICartRepository>();
        _cartCacheServiceMock = new Mock<ICartCacheService>();
        Mock<ILogger<GetCartQueryHandler>> loggerMock = new();
        
        _handler = new GetCartQueryHandler(
            _cartRepositoryMock.Object,
            _cartCacheServiceMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnCartFromCache_WhenExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetCartQuery(userId);
        var cachedCart = new CartResponse
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Items = new List<CartItemResponse>(),
            TotalAmount = 0,
            TotalItems = 0
        };

        _cartCacheServiceMock.Setup(x => x.GetCartAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedCart);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(cachedCart.Id, result.Id);
        _cartRepositoryMock.Verify(x => x.GetByUserIdWithItemsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnCartFromDatabase_WhenNotInCache()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetCartQuery(userId);
        var categoryId = Guid.NewGuid();
        var cart = Cart.Create(userId);
        cart.AddItem(Product.Create("Test", categoryId, price: 100m, stock: 10), 2);

        _cartCacheServiceMock.Setup(x => x.GetCartAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CartResponse?)null);
        _cartRepositoryMock.Setup(x => x.GetByUserIdWithItemsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(cart.Id, result.Id);
        Assert.Equal(2, result.TotalItems);
        _cartCacheServiceMock.Verify(x => x.SetCartAsync(userId, It.IsAny<CartResponse>(), null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenCartNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetCartQuery(userId);

        _cartCacheServiceMock.Setup(x => x.GetCartAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CartResponse?)null);
        _cartRepositoryMock.Setup(x => x.GetByUserIdWithItemsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cart?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }
}