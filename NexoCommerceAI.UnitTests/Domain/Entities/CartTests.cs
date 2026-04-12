using NexoCommerceAI.Domain.Entities;
using NexoCommerceAI.Domain.Events;
using Xunit;

namespace NexoCommerceAI.UnitTests.Domain.Entities;

public class CartTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _productId = Guid.NewGuid();
    private readonly Product _product;

    public CartTests()
    {
        var categoryId = Guid.NewGuid();
        _product = Product.Create("Test Product", categoryId, price: 100m, stock: 10);
    }

    [Fact]
    public void Create_ShouldCreateCart_WhenValidUserId()
    {
        // Act
        var cart = Cart.Create(_userId);

        // Assert
        Assert.Equal(_userId, cart.UserId);
        Assert.True(cart.IsActive);
        Assert.NotNull(cart.LastUpdatedAt);
        Assert.Empty(cart.Items);
        Assert.Equal(0, cart.TotalAmount);
        Assert.Equal(0, cart.TotalItems);
        Assert.False(cart.IsAbandoned);
        Assert.Null(cart.AbandonedAt);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenUserIdIsEmpty()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Cart.Create(Guid.Empty));
    }

    [Fact]
    public void AddItem_ShouldAddNewItem_WhenProductNotInCart()
    {
        // Arrange
        var cart = Cart.Create(_userId);
        var quantity = 2;

        // Act
        cart.AddItem(_product, quantity);

        // Assert
        Assert.Single(cart.Items);
        var item = cart.Items.First();
        Assert.Equal(_product.Id, item.ProductId);
        Assert.Equal(_product.Name, item.ProductName);
        Assert.Equal(quantity, item.Quantity);
        Assert.Equal(_product.Price, item.UnitPrice);
        Assert.Equal(_product.Price * quantity, item.TotalPrice);
        Assert.Equal(_product.Price * quantity, cart.TotalAmount);
        Assert.Equal(quantity, cart.TotalItems);
        Assert.False(cart.IsAbandoned);
        Assert.Null(cart.AbandonedAt);
    }

    [Fact]
    public void AddItem_ShouldUpdateQuantity_WhenProductAlreadyInCart()
    {
        // Arrange
        var cart = Cart.Create(_userId);
        cart.AddItem(_product, 2);

        // Act
        cart.AddItem(_product, 3);

        // Assert
        Assert.Single(cart.Items);
        var item = cart.Items.First();
        Assert.Equal(5, item.Quantity);
        Assert.Equal(_product.Price * 5, cart.TotalAmount);
        Assert.Equal(5, cart.TotalItems);
    }

    [Fact]
    public void AddItem_ShouldThrowException_WhenQuantityIsZeroOrNegative()
    {
        // Arrange
        var cart = Cart.Create(_userId);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => cart.AddItem(_product, 0));
        Assert.Throws<ArgumentException>(() => cart.AddItem(_product, -5));
    }

    [Fact]
    public void AddItem_ShouldThrowException_WhenProductIsNull()
    {
        // Arrange
        var cart = Cart.Create(_userId);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => cart.AddItem(null!, 1));
    }

    [Fact]
    public void AddItem_ShouldUseSelectedPrice_WhenProvided()
    {
        // Arrange
        var cart = Cart.Create(_userId);
        var selectedPrice = 80m;

        // Act
        cart.AddItem(_product, 2, selectedPrice);

        // Assert
        var item = cart.Items.First();
        Assert.Equal(selectedPrice, item.UnitPrice);
        Assert.Equal(selectedPrice * 2, item.TotalPrice);
        Assert.Equal(selectedPrice * 2, cart.TotalAmount);
    }

    [Fact]
    public void AddItem_ShouldGenerateCartItemAddedEvent()
    {
        // Arrange
        var cart = Cart.Create(_userId);

        // Act
        cart.AddItem(_product, 2);

        // Assert
        Assert.Contains(cart.DomainEvents, e => e is CartItemAddedEvent);
        var @event = cart.DomainEvents.First(e => e is CartItemAddedEvent) as CartItemAddedEvent;
        Assert.Equal(_userId, @event!.UserId);
        Assert.Equal(_product.Id, @event.ProductId);
        Assert.Equal(2, @event.Quantity);
    }

    [Fact]
    public void UpdateItemQuantity_ShouldUpdateQuantity_WhenValid()
    {
        // Arrange
        var cart = Cart.Create(_userId);
        cart.AddItem(_product, 2);

        // Act
        cart.UpdateItemQuantity(_product.Id, 5);

        // Assert
        var item = cart.Items.First();
        Assert.Equal(5, item.Quantity);
        Assert.Equal(_product.Price * 5, cart.TotalAmount);
        Assert.Equal(5, cart.TotalItems);
    }

    [Fact]
    public void UpdateItemQuantity_ShouldRemoveItem_WhenQuantityIsZero()
    {
        // Arrange
        var cart = Cart.Create(_userId);
        cart.AddItem(_product, 2);

        // Act
        cart.UpdateItemQuantity(_product.Id, 0);

        // Assert
        Assert.Empty(cart.Items);
        Assert.Equal(0, cart.TotalAmount);
        Assert.Equal(0, cart.TotalItems);
    }

    [Fact]
    public void UpdateItemQuantity_ShouldThrowException_WhenProductNotFound()
    {
        // Arrange
        var cart = Cart.Create(_userId);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => cart.UpdateItemQuantity(Guid.NewGuid(), 5));
    }

    [Fact]
    public void RemoveItem_ShouldRemoveItem_WhenProductExists()
    {
        // Arrange
        var cart = Cart.Create(_userId);
        cart.AddItem(_product, 2);
        cart.AddItem(Product.Create("Other Product", Guid.NewGuid(), price: 50m, stock: 5), 1);

        // Act
        cart.RemoveItem(_product.Id);

        // Assert
        Assert.Single(cart.Items);
        Assert.Equal(0, cart.TotalAmount);
        Assert.Equal(0, cart.TotalItems);
    }

    [Fact]
    public void RemoveItem_ShouldThrowException_WhenProductNotFound()
    {
        // Arrange
        var cart = Cart.Create(_userId);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => cart.RemoveItem(Guid.NewGuid()));
    }

    [Fact]
    public void Clear_ShouldRemoveAllItems()
    {
        // Arrange
        var cart = Cart.Create(_userId);
        cart.AddItem(_product, 2);
        cart.AddItem(Product.Create("Other Product", Guid.NewGuid(), price: 50m, stock: 5), 1);

        // Act
        cart.Clear();

        // Assert
        Assert.Empty(cart.Items);
        Assert.Equal(0, cart.TotalAmount);
        Assert.Equal(0, cart.TotalItems);
    }

    [Fact]
    public void MarkAsAbandoned_ShouldMarkCartAsAbandoned()
    {
        // Arrange
        var cart = Cart.Create(_userId);
        cart.AddItem(_product, 2);

        // Act
        cart.MarkAsAbandoned();

        // Assert
        Assert.True(cart.IsAbandoned);
        Assert.NotNull(cart.AbandonedAt);
        Assert.Contains(cart.DomainEvents, e => e is CartAbandonedEvent);
    }

    [Fact]
    public void MergeCart_ShouldMergeGuestCartIntoUserCart()
    {
        // Arrange
        var userCart = Cart.Create(_userId);
        userCart.AddItem(_product, 2);

        var guestCart = Cart.Create(Guid.NewGuid());
        var otherProduct = Product.Create("Other Product", Guid.NewGuid(), price: 50m, stock: 5);
        guestCart.AddItem(otherProduct, 1);

        // Act
        userCart.MergeCart(guestCart);

        // Assert
        Assert.Equal(2, userCart.Items.Count);
        Assert.Contains(userCart.Items, i => i.ProductId == _product.Id);
        Assert.Contains(userCart.Items, i => i.ProductId == otherProduct.Id);
        Assert.Equal(2, userCart.Items.First(i => i.ProductId == _product.Id).Quantity);
        Assert.Equal(1, userCart.Items.First(i => i.ProductId == otherProduct.Id).Quantity);
    }
}