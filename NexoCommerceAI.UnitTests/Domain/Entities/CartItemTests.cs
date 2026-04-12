using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Domain.Entities;

public class CartItemTests
{
    private readonly Guid _cartId = Guid.NewGuid();
    private readonly Guid _productId = Guid.NewGuid();

    [Fact]
    public void Create_ShouldCreateCartItem_WhenValidParameters()
    {
        // Act
        var item = CartItem.Create(_cartId, _productId, "Test Product", 100m, "https://image.jpg", 2);

        // Assert
        Assert.Equal(_cartId, item.CartId);
        Assert.Equal(_productId, item.ProductId);
        Assert.Equal("Test Product", item.ProductName);
        Assert.Equal(100m, item.UnitPrice);
        Assert.Equal("https://image.jpg", item.ProductImageUrl);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(200m, item.TotalPrice);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenCartIdIsEmpty()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            CartItem.Create(Guid.Empty, _productId, "Test", 100m, null, 1));
    }

    [Fact]
    public void Create_ShouldThrowException_WhenProductIdIsEmpty()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            CartItem.Create(_cartId, Guid.Empty, "Test", 100m, null, 1));
    }

    [Fact]
    public void Create_ShouldThrowException_WhenProductNameIsEmpty()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            CartItem.Create(_cartId, _productId, "", 100m, null, 1));
        Assert.Throws<ArgumentException>(() => 
            CartItem.Create(_cartId, _productId, null!, 100m, null, 1));
    }

    [Fact]
    public void Create_ShouldThrowException_WhenUnitPriceIsZeroOrNegative()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            CartItem.Create(_cartId, _productId, "Test", 0, null, 1));
        Assert.Throws<ArgumentException>(() => 
            CartItem.Create(_cartId, _productId, "Test", -10m, null, 1));
    }

    [Fact]
    public void Create_ShouldThrowException_WhenQuantityIsZeroOrNegative()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            CartItem.Create(_cartId, _productId, "Test", 100m, null, 0));
        Assert.Throws<ArgumentException>(() => 
            CartItem.Create(_cartId, _productId, "Test", 100m, null, -5));
    }

    [Fact]
    public void UpdateQuantity_ShouldUpdateQuantity()
    {
        // Arrange
        var item = CartItem.Create(_cartId, _productId, "Test", 100m, null, 2);

        // Act
        item.UpdateQuantity(5);

        // Assert
        Assert.Equal(5, item.Quantity);
        Assert.Equal(500m, item.TotalPrice);
        Assert.NotNull(item.UpdatedAt);
    }

    [Fact]
    public void UpdateQuantity_ShouldThrowException_WhenQuantityIsZeroOrNegative()
    {
        // Arrange
        var item = CartItem.Create(_cartId, _productId, "Test", 100m, null, 2);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => item.UpdateQuantity(0));
        Assert.Throws<ArgumentException>(() => item.UpdateQuantity(-5));
    }

    [Fact]
    public void UpdateUnitPrice_ShouldUpdateUnitPrice()
    {
        // Arrange
        var item = CartItem.Create(_cartId, _productId, "Test", 100m, null, 2);

        // Act
        item.UpdateUnitPrice(80m);

        // Assert
        Assert.Equal(80m, item.UnitPrice);
        Assert.Equal(160m, item.TotalPrice);
        Assert.NotNull(item.UpdatedAt);
    }

    [Fact]
    public void UpdateUnitPrice_ShouldThrowException_WhenPriceIsZeroOrNegative()
    {
        // Arrange
        var item = CartItem.Create(_cartId, _productId, "Test", 100m, null, 2);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => item.UpdateUnitPrice(0));
        Assert.Throws<ArgumentException>(() => item.UpdateUnitPrice(-10m));
    }
}