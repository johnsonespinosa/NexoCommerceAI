using NexoCommerceAI.Domain.Entities;
using NexoCommerceAI.Infrastructure.Data.Repositories;
using NexoCommerceAI.IntegrationTests.Helpers;
using Xunit;

namespace NexoCommerceAI.IntegrationTests.Infrastructure.Repositories;

public class WishlistRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly WishlistRepository _repository;

    public WishlistRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new WishlistRepository(_fixture.Context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddWishlistToDatabase()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var wishlist = Wishlist.CreateDefault(userId);

        // Act
        await _repository.AddAsync(wishlist);
        await _repository.SaveChangesAsync();

        // Assert
        var savedWishlist = await _fixture.Context.Wishlists.FindAsync(wishlist.Id);
        Assert.NotNull(savedWishlist);
        Assert.Equal(userId, savedWishlist.UserId);
        Assert.Equal("Default Wishlist", savedWishlist.Name);
    }

    [Fact]
    public async Task GetDefaultByUserIdAsync_ShouldReturnDefaultWishlist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var wishlist = Wishlist.CreateDefault(userId);
        await _repository.AddAsync(wishlist);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetDefaultByUserIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsDefault);
        Assert.Equal("Default Wishlist", result.Name);
    }

    [Fact]
    public async Task GetByUserIdWithItemsAsync_ShouldReturnWishlistWithItems()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var wishlist = Wishlist.CreateDefault(userId);
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", categoryId, price: 100m, stock: 10);
        
        await _fixture.Context.Products.AddAsync(product);
        await _fixture.Context.Wishlists.AddAsync(wishlist);
        await _fixture.Context.SaveChangesAsync();
        
        wishlist.AddItem(product);
        await _repository.UpdateAsync(wishlist);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdWithItemsAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(product.Id, result.Items.First().ProductId);
    }

    [Fact]
    public async Task IsInWishlistAsync_ShouldReturnTrue_WhenProductInWishlist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var wishlist = Wishlist.CreateDefault(userId);
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", categoryId, price: 100m, stock: 10);
        
        await _fixture.Context.Products.AddAsync(product);
        await _fixture.Context.Wishlists.AddAsync(wishlist);
        await _fixture.Context.SaveChangesAsync();
        
        wishlist.AddItem(product);
        await _repository.UpdateAsync(wishlist);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.IsInWishlistAsync(userId, product.Id);

        // Assert
        Assert.True(result);
    }
}