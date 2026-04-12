using NexoCommerceAI.Domain.Entities;
using NexoCommerceAI.Infrastructure.Data.Repositories;
using NexoCommerceAI.IntegrationTests.Helpers;
using Xunit;

namespace NexoCommerceAI.IntegrationTests.Infrastructure.Repositories;

public class CartRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly CartRepository _repository;

    public CartRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new CartRepository(_fixture.Context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddCartToDatabase()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cart = Cart.Create(userId);

        // Act
        await _repository.AddAsync(cart);
        await _repository.SaveChangesAsync();

        // Assert
        var savedCart = await _fixture.Context.Carts.FindAsync(cart.Id);
        Assert.NotNull(savedCart);
        Assert.Equal(userId, savedCart.UserId);
    }

    [Fact]
    public async Task GetByUserIdWithItemsAsync_ShouldReturnCartWithItems()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cart = Cart.Create(userId);
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", categoryId, price: 100m, stock: 10);
        
        await _fixture.Context.Products.AddAsync(product);
        await _fixture.Context.Carts.AddAsync(cart);
        await _fixture.Context.SaveChangesAsync();
        
        cart.AddItem(product, 2);
        await _repository.UpdateAsync(cart);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdWithItemsAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(product.Id, result.Items.First().ProductId);
        Assert.Equal(2, result.Items.First().Quantity);
    }

    [Fact]
    public async Task GetAbandonedCartsAsync_ShouldReturnAbandonedCarts()
    {
        // Arrange
        var oldDate = DateTime.UtcNow.AddHours(-25);
        
        var userId1 = Guid.NewGuid();
        var cart1 = Cart.Create(userId1);
        cart1.AddItem(Product.Create("Product1", Guid.NewGuid(), price: 100m, stock: 10), 1);
        
        var userId2 = Guid.NewGuid();
        var cart2 = Cart.Create(userId2);
        cart2.AddItem(Product.Create("Product2", Guid.NewGuid(), price: 100m, stock: 10), 1);
        
        // Forzar LastUpdatedAt antiguo
        var field = typeof(Cart).GetProperty("LastUpdatedAt");
        field?.SetValue(cart1, oldDate);
        field?.SetValue(cart2, oldDate);
        
        await _fixture.Context.Carts.AddRangeAsync(cart1, cart2);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAbandonedCartsAsync(TimeSpan.FromHours(24));

        // Assert
        Assert.Equal(2, result.Count);
    }
}