using NexoCommerceAI.Domain.Entities;
using NexoCommerceAI.Domain.Enums;
using NexoCommerceAI.Domain.Events;
using NexoCommerceAI.Domain.ValueObjects;
using Xunit;

namespace NexoCommerceAI.UnitTests.Domain.Entities;

public class OrderTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Address _address;
    private readonly Product _product;

    public OrderTests()
    {
        var categoryId = Guid.NewGuid();
        _product = Product.Create("Test Product", categoryId, price: 100m, stock: 10);
        _address = new Address("123 Main St", "New York", "NY", "10001", "USA");
    }

    [Fact]
    public void Create_ShouldCreateOrder_WhenValidParameters()
    {
        // Act
        var order = Order.Create(_userId, _address, _address);

        // Assert
        Assert.NotEqual(Guid.Empty, order.Id);
        Assert.NotNull(order.OrderNumber);
        Assert.StartsWith("ORD-", order.OrderNumber);
        Assert.Equal(_userId, order.UserId);
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Empty(order.Items);
        Assert.Equal(0, order.TotalAmount.Amount);
        Assert.Contains(order.DomainEvents, e => e is OrderCreatedEvent);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenUserIdIsEmpty()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Order.Create(Guid.Empty, _address, _address));
    }

    [Fact]
    public void AddItem_ShouldAddItemToOrder()
    {
        // Arrange
        var order = Order.Create(_userId, _address, _address);

        // Act
        order.AddItem(_product, 2, 100m);

        // Assert
        Assert.Single(order.Items);
        var item = order.Items.First();
        Assert.Equal(_product.Id, item.ProductId);
        Assert.Equal(_product.Name, item.ProductName);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(200m, item.TotalPrice);
        Assert.Equal(200m, order.Subtotal.Amount);
    }

    [Fact]
    public void AddItem_ShouldRecalculateTotals()
    {
        // Arrange
        var order = Order.Create(_userId, _address, _address);
        var product2 = Product.Create("Product 2", Guid.NewGuid(), price: 50m, stock: 5);

        // Act
        order.AddItem(_product, 2, 100m);
        order.AddItem(product2, 1, 50m);

        // Assert
        Assert.Equal(2, order.Items.Count);
        Assert.Equal(250m, order.Subtotal.Amount);
        Assert.Equal(52.5m, order.TaxAmount.Amount); // 21% IVA
        Assert.Equal(302.5m, order.TotalAmount.Amount);
    }

    [Fact]
    public void UpdateStatus_ShouldChangeStatus_WhenValidTransition()
    {
        // Arrange
        var order = Order.Create(_userId, _address, _address);

        // Act
        order.UpdateStatus(OrderStatus.PaymentProcessing);

        // Assert
        Assert.Equal(OrderStatus.PaymentProcessing, order.Status);
        Assert.Single(order.StatusHistory);
        Assert.Contains(order.DomainEvents, e => e is OrderStatusChangedEvent);
    }

    [Fact]
    public void UpdateStatus_ShouldThrowException_WhenInvalidTransition()
    {
        // Arrange
        var order = Order.Create(_userId, _address, _address);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.UpdateStatus(OrderStatus.Shipped));
    }

    [Fact]
    public void UpdateStatus_ShouldSetShippedAt_WhenStatusIsShipped()
    {
        // Arrange
        var order = Order.Create(_userId, _address, _address);
        order.UpdateStatus(OrderStatus.PaymentProcessing);
        order.UpdateStatus(OrderStatus.Paid);

        // Act
        order.UpdateStatus(OrderStatus.Shipped);

        // Assert
        Assert.Equal(OrderStatus.Shipped, order.Status);
        Assert.NotNull(order.ShippedAt);
        Assert.Contains(order.DomainEvents, e => e is OrderShippedEvent);
    }

    [Fact]
    public void UpdateShippingInfo_ShouldUpdateTrackingInfo()
    {
        // Arrange
        var order = Order.Create(_userId, _address, _address);
        order.UpdateStatus(OrderStatus.PaymentProcessing);
        order.UpdateStatus(OrderStatus.Paid);

        // Act
        order.UpdateShippingInfo("1Z999AA10123456784", "UPS");

        // Assert
        Assert.Equal("1Z999AA10123456784", order.TrackingNumber);
        Assert.Equal("UPS", order.CarrierName);
        Assert.Equal(OrderStatus.Shipped, order.Status);
    }

    [Fact]
    public void Cancel_ShouldCancelOrder_WhenPending()
    {
        // Arrange
        var order = Order.Create(_userId, _address, _address);

        // Act
        order.Cancel("Customer requested cancellation");

        // Assert
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.NotNull(order.CancelledAt);
        Assert.Contains(order.DomainEvents, e => e is OrderCancelledEvent);
    }

    [Fact]
    public void Cancel_ShouldThrowException_WhenOrderIsDelivered()
    {
        // Arrange
        var order = Order.Create(_userId, _address, _address);
        order.UpdateStatus(OrderStatus.PaymentProcessing);
        order.UpdateStatus(OrderStatus.Paid);
        order.UpdateStatus(OrderStatus.Shipped);
        order.UpdateStatus(OrderStatus.Delivered);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.Cancel());
    }

    [Fact]
    public void AddAdminNotes_ShouldAddNotes()
    {
        // Arrange
        var order = Order.Create(_userId, _address, _address);
        var notes = "Payment verified, processing order";

        // Act
        order.AddAdminNotes(notes);

        // Assert
        Assert.Equal(notes, order.AdminNotes);
        Assert.NotNull(order.UpdatedAt);
    }
}