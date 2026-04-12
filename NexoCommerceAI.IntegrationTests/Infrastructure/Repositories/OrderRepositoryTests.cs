using NexoCommerceAI.Domain.Entities;
using NexoCommerceAI.Domain.Enums;
using NexoCommerceAI.Domain.ValueObjects;
using NexoCommerceAI.Infrastructure.Data.Repositories;
using NexoCommerceAI.IntegrationTests.Helpers;
using Xunit;

namespace NexoCommerceAI.IntegrationTests.Infrastructure.Repositories;

public class OrderRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly OrderRepository _repository;

    public OrderRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new OrderRepository(_fixture.Context);
    }

    private static Address CreateAddress() => new("123 Main St", "New York", "NY", "10001", "USA");

    [Fact]
    public async Task AddAsync_ShouldAddOrderToDatabase()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var address = CreateAddress();
        var order = Order.Create(userId, address, address);

        // Act
        await _repository.AddAsync(order);
        await _repository.SaveChangesAsync();

        // Assert
        var savedOrder = await _fixture.Context.Orders.FindAsync(order.Id);
        Assert.NotNull(savedOrder);
        Assert.Equal(order.OrderNumber, savedOrder.OrderNumber);
        Assert.Equal(userId, savedOrder.UserId);
    }

    [Fact]
    public async Task GetByIdWithItemsAsync_ShouldReturnOrderWithItems()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var address = CreateAddress();
        var order = Order.Create(userId, address, address);
        
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", categoryId, price: 100m, stock: 10);
        await _fixture.Context.Products.AddAsync(product);
        await _fixture.Context.Orders.AddAsync(order);
        await _fixture.Context.SaveChangesAsync();
        
        order.AddItem(product, 2, 100m);
        await _repository.UpdateAsync(order);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdWithItemsAsync(order.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(product.Id, result.Items.First().ProductId);
        Assert.Equal(2, result.Items.First().Quantity);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnUserOrders()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var address = CreateAddress();
        
        var order1 = Order.Create(userId, address, address);
        var order2 = Order.Create(userId, address, address);
        
        await _fixture.Context.Orders.AddRangeAsync(order1, order2);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAsync(userId, 1, 10);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, o => Assert.Equal(userId, o.UserId));
    }

    [Fact]
    public async Task GetByStatusAsync_ShouldReturnOrdersByStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var address = CreateAddress();
        
        var order = Order.Create(userId, address, address);
        
        await _fixture.Context.Orders.AddAsync(order);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByStatusAsync(OrderStatus.Pending);

        // Assert
        Assert.Single(result);
        Assert.Equal(OrderStatus.Pending, result.First().Status);
    }
}