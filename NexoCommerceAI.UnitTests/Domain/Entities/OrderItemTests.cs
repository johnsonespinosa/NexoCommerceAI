using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Domain.Entities;

public class OrderItemTests
{
    private readonly Guid _orderId = Guid.NewGuid();
    private readonly Guid _productId = Guid.NewGuid();

    [Fact]
    public void Create_ShouldCreateOrderItem_WhenValidParameters()
    {
        // Act
        var item = OrderItem.Create(_orderId, _productId, "Test Product", "SKU-001", 100m, 2, "https://image.jpg");

        // Assert
        Assert.Equal(_orderId, item.OrderId);
        Assert.Equal(_productId, item.ProductId);
        Assert.Equal("Test Product", item.ProductName);
        Assert.Equal("SKU-001", item.ProductSku);
        Assert.Equal(100m, item.UnitPrice);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(200m, item.TotalPrice);
        Assert.Equal("https://image.jpg", item.ProductImageUrl);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenOrderIdIsEmpty()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            OrderItem.Create(Guid.Empty, _productId, "Test", "SKU-001", 100m, 1));
    }

    [Fact]
    public void Create_ShouldThrowException_WhenProductIdIsEmpty()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            OrderItem.Create(_orderId, Guid.Empty, "Test", "SKU-001", 100m, 1));
    }

    [Fact]
    public void Create_ShouldThrowException_WhenProductNameIsEmpty()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            OrderItem.Create(_orderId, _productId, "", "SKU-001", 100m, 1));
    }

    [Fact]
    public void Create_ShouldThrowException_WhenProductSkuIsEmpty()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            OrderItem.Create(_orderId, _productId, "Test", "", 100m, 1));
    }

    [Fact]
    public void Create_ShouldThrowException_WhenUnitPriceIsZeroOrNegative()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            OrderItem.Create(_orderId, _productId, "Test", "SKU-001", 0, 1));
        Assert.Throws<ArgumentException>(() => 
            OrderItem.Create(_orderId, _productId, "Test", "SKU-001", -10m, 1));
    }

    [Fact]
    public void Create_ShouldThrowException_WhenQuantityIsZeroOrNegative()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            OrderItem.Create(_orderId, _productId, "Test", "SKU-001", 100m, 0));
        Assert.Throws<ArgumentException>(() => 
            OrderItem.Create(_orderId, _productId, "Test", "SKU-001", 100m, -5));
    }

    [Fact]
    public void UpdateQuantity_ShouldUpdateQuantity()
    {
        // Arrange
        var item = OrderItem.Create(_orderId, _productId, "Test", "SKU-001", 100m, 2);

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
        var item = OrderItem.Create(_orderId, _productId, "Test", "SKU-001", 100m, 2);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => item.UpdateQuantity(0));
        Assert.Throws<ArgumentException>(() => item.UpdateQuantity(-5));
    }
}