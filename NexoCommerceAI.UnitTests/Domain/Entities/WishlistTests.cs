using NexoCommerceAI.Domain.Entities;
using NexoCommerceAI.Domain.Events;
using Xunit;

namespace NexoCommerceAI.UnitTests.Domain.Entities;

public class WishlistTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _productId = Guid.NewGuid();
    private readonly Product _product;

    public WishlistTests()
    {
        var categoryId = Guid.NewGuid();
        _product = Product.Create("Test Product", categoryId, price: 100m, stock: 10);
    }

    [Fact]
    public void CreateDefault_ShouldCreateDefaultWishlist()
    {
        // Act
        var wishlist = Wishlist.CreateDefault(_userId);

        // Assert
        Assert.Equal(_userId, wishlist.UserId);
        Assert.Equal("Default Wishlist", wishlist.Name);
        Assert.True(wishlist.IsDefault);
        Assert.Empty(wishlist.Items);
        Assert.Equal(0, wishlist.TotalItems);
        Assert.True(wishlist.IsActive);
    }

    [Fact]
    public void CreateDefault_ShouldThrowException_WhenUserIdIsEmpty()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Wishlist.CreateDefault(Guid.Empty));
    }

    [Fact]
    public void CreateCustom_ShouldCreateCustomWishlist()
    {
        // Arrange
        var name = "My Favorites";

        // Act
        var wishlist = Wishlist.CreateCustom(_userId, name);

        // Assert
        Assert.Equal(_userId, wishlist.UserId);
        Assert.Equal(name, wishlist.Name);
        Assert.False(wishlist.IsDefault);
        Assert.Empty(wishlist.Items);
        Assert.Equal(0, wishlist.TotalItems);
    }

    [Fact]
    public void CreateCustom_ShouldThrowException_WhenNameIsEmpty()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Wishlist.CreateCustom(_userId, ""));
        Assert.Throws<ArgumentException>(() => Wishlist.CreateCustom(_userId, null!));
    }

    [Fact]
    public void AddItem_ShouldAddProductToWishlist()
    {
        // Arrange
        var wishlist = Wishlist.CreateDefault(_userId);

        // Act
        wishlist.AddItem(_product);

        // Assert
        Assert.Single(wishlist.Items);
        var item = wishlist.Items.First();
        Assert.Equal(_product.Id, item.ProductId);
        Assert.Equal(_product.Name, item.ProductName);
        Assert.Equal(_product.Price, item.Price);
        Assert.Equal(_product.ImageUrl, item.ProductImageUrl);
        Assert.Equal(1, wishlist.TotalItems);
    }

    [Fact]
    public void AddItem_ShouldThrowException_WhenProductAlreadyInWishlist()
    {
        // Arrange
        var wishlist = Wishlist.CreateDefault(_userId);
        wishlist.AddItem(_product);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => wishlist.AddItem(_product));
    }

    [Fact]
    public void AddItem_ShouldThrowException_WhenProductIsNull()
    {
        // Arrange
        var wishlist = Wishlist.CreateDefault(_userId);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => wishlist.AddItem(null!));
    }

    [Fact]
    public void AddItem_ShouldGenerateWishlistItemAddedEvent()
    {
        // Arrange
        var wishlist = Wishlist.CreateDefault(_userId);

        // Act
        wishlist.AddItem(_product);

        // Assert
        Assert.Contains(wishlist.DomainEvents, e => e is WishlistItemAddedEvent);
        var @event = wishlist.DomainEvents.First(e => e is WishlistItemAddedEvent) as WishlistItemAddedEvent;
        Assert.Equal(_userId, @event!.UserId);
        Assert.Equal(_product.Id, @event.ProductId);
        Assert.Equal(_product.Name, @event.ProductName);
    }

    [Fact]
    public void RemoveItem_ShouldRemoveProductFromWishlist()
    {
        // Arrange
        var wishlist = Wishlist.CreateDefault(_userId);
        wishlist.AddItem(_product);
        var otherProduct = Product.Create("Other", Guid.NewGuid(), price: 50m, stock: 5);
        wishlist.AddItem(otherProduct);

        // Act
        wishlist.RemoveItem(_product.Id);

        // Assert
        Assert.Single(wishlist.Items);
        Assert.Equal(otherProduct.Id, wishlist.Items.First().ProductId);
        Assert.Equal(1, wishlist.TotalItems);
    }

    [Fact]
    public void RemoveItem_ShouldThrowException_WhenProductNotFound()
    {
        // Arrange
        var wishlist = Wishlist.CreateDefault(_userId);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => wishlist.RemoveItem(Guid.NewGuid()));
    }

    [Fact]
    public void Contains_ShouldReturnTrue_WhenProductInWishlist()
    {
        // Arrange
        var wishlist = Wishlist.CreateDefault(_userId);
        wishlist.AddItem(_product);

        // Act & Assert
        Assert.True(wishlist.Contains(_product.Id));
        Assert.False(wishlist.Contains(Guid.NewGuid()));
    }

    [Fact]
    public void Rename_ShouldUpdateWishlistName()
    {
        // Arrange
        var wishlist = Wishlist.CreateDefault(_userId);
        var newName = "New Wishlist Name";

        // Act
        wishlist.Rename(newName);

        // Assert
        Assert.Equal(newName, wishlist.Name);
        Assert.NotNull(wishlist.UpdatedAt);
    }

    [Fact]
    public void Rename_ShouldThrowException_WhenNameIsEmpty()
    {
        // Arrange
        var wishlist = Wishlist.CreateDefault(_userId);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => wishlist.Rename(""));
        Assert.Throws<ArgumentException>(() => wishlist.Rename(null!));
    }
}