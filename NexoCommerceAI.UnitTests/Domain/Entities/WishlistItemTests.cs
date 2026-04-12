using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Domain.Entities;

public class WishlistItemTests
{
    private readonly Guid _wishlistId = Guid.NewGuid();
    private readonly Guid _productId = Guid.NewGuid();

    [Fact]
    public void Create_ShouldCreateWishlistItem_WhenValidParameters()
    {
        // Act
        var item = WishlistItem.Create(_wishlistId, _productId, "Test Product", 100m, "https://image.jpg", "My notes");

        // Assert
        Assert.Equal(_wishlistId, item.WishlistId);
        Assert.Equal(_productId, item.ProductId);
        Assert.Equal("Test Product", item.ProductName);
        Assert.Equal(100m, item.Price);
        Assert.Equal("https://image.jpg", item.ProductImageUrl);
        Assert.Equal("My notes", item.Notes);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenWishlistIdIsEmpty()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            WishlistItem.Create(Guid.Empty, _productId, "Test", 100m, null));
    }

    [Fact]
    public void Create_ShouldThrowException_WhenProductIdIsEmpty()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            WishlistItem.Create(_wishlistId, Guid.Empty, "Test", 100m, null));
    }

    [Fact]
    public void Create_ShouldThrowException_WhenProductNameIsEmpty()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            WishlistItem.Create(_wishlistId, _productId, "", 100m, null));
        Assert.Throws<ArgumentException>(() => 
            WishlistItem.Create(_wishlistId, _productId, null!, 100m, null));
    }

    [Fact]
    public void Create_ShouldThrowException_WhenPriceIsZeroOrNegative()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            WishlistItem.Create(_wishlistId, _productId, "Test", 0, null));
        Assert.Throws<ArgumentException>(() => 
            WishlistItem.Create(_wishlistId, _productId, "Test", -10m, null));
    }

    [Fact]
    public void AddNotes_ShouldUpdateNotes()
    {
        // Arrange
        var item = WishlistItem.Create(_wishlistId, _productId, "Test", 100m, null);
        const string newNotes = "Updated notes";

        // Act
        item.AddNotes(newNotes);

        // Assert
        Assert.Equal(newNotes, item.Notes);
        Assert.NotNull(item.UpdatedAt);
    }
}