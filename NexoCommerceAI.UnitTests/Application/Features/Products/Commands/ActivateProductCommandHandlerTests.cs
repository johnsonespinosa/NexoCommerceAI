using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Commands;
using NexoCommerceAI.Application.Features.Products.Handlers;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Products.Commands;

public class ActivateProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ILogger<ActivateProductCommandHandler>> _loggerMock;
    private readonly ActivateProductCommandHandler _handler;
    
    public ActivateProductCommandHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _loggerMock = new Mock<ILogger<ActivateProductCommandHandler>>();
        _handler = new ActivateProductCommandHandler(_productRepositoryMock.Object, _loggerMock.Object);
    }
    
    [Fact]
    public async Task Handle_WithInactiveProduct_ShouldActivate()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", categoryId, price: 100m, stock: 10);
        product.Deactivate(); // Producto inactivo
        var command = new ActivateProductCommand(productId);
        
        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.True(result);
        Assert.True(product.IsActive);
        _productRepositoryMock.Verify(x => x.UpdateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_WithAlreadyActiveProduct_ShouldReturnFalse()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", categoryId, price: 100m, stock: 10);
        var command = new ActivateProductCommand(productId);
        
        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.False(result);
        Assert.True(product.IsActive); // Sigue activo
        _productRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task Handle_WithDeletedProduct_ShouldThrowValidationException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", categoryId, price: 100m, stock: 10);
        product.SoftDelete();
        var command = new ActivateProductCommand(productId);
        
        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => 
            _handler.Handle(command, CancellationToken.None));
    }
}