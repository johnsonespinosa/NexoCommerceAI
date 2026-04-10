using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Commands;
using NexoCommerceAI.Application.Features.Products.Handlers;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Products.Commands;

public class UpdateProductStockCommandTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly UpdateProductStockCommandHandler _handler;
    
    public UpdateProductStockCommandTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        Mock<ILogger<UpdateProductStockCommandHandler>> loggerMock = new();
        _handler = new UpdateProductStockCommandHandler(_productRepositoryMock.Object, loggerMock.Object);
    }
    
    [Fact]
    public async Task Handle_WithValidProduct_ShouldUpdateStock()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", categoryId, price: 100m, stock: 10);
        var command = new UpdateProductStockCommand { ProductId = productId, NewStock = 25 };
        
        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.True(result);
        Assert.Equal(25, product.Stock);
        _productRepositoryMock.Verify(x => x.UpdateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
        _productRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_WithNonExistingProduct_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new UpdateProductStockCommand { ProductId = Guid.NewGuid(), NewStock = 25 };
        
        _productRepositoryMock.Setup(x => x.GetByIdAsync(command.ProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);
        
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            _handler.Handle(command, CancellationToken.None));
    }
    
    [Fact]
    public async Task Handle_WithZeroStock_ShouldUpdateToZero()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", categoryId, price: 100m, stock: 10);
        var command = new UpdateProductStockCommand { ProductId = productId, NewStock = 0 };
        
        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.True(result);
        Assert.Equal(0, product.Stock);
    }
}