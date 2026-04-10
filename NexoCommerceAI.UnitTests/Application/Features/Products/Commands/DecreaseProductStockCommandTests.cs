using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Commands;
using NexoCommerceAI.Application.Features.Products.Handlers;
using NexoCommerceAI.Application.Features.Products.Validators;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Products.Commands;

public class DecreaseProductStockCommandTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ILogger<DecreaseProductStockCommandHandler>> _loggerMock;
    private readonly DecreaseProductStockCommandHandler _handler;
    
    public DecreaseProductStockCommandTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _loggerMock = new Mock<ILogger<DecreaseProductStockCommandHandler>>();
        _handler = new DecreaseProductStockCommandHandler(_productRepositoryMock.Object, _loggerMock.Object);
    }
    
    [Fact]
    public async Task Handle_WithSufficientStock_ShouldDecreaseStock()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", categoryId, price: 100m, stock: 10);
        var command = new DecreaseProductStockCommand(productId, 3);
        
        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.True(result);
        Assert.Equal(7, product.Stock);
        _productRepositoryMock.Verify(x => x.UpdateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
        _productRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_WithExactStock_ShouldSetStockToZero()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", categoryId, price: 100m, stock: 5);
        var command = new DecreaseProductStockCommand(productId, 5);
        
        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.True(result);
        Assert.Equal(0, product.Stock);
        Assert.False(product.IsInStock());
    }
    
    [Fact]
    public async Task Handle_WithInsufficientStock_ShouldThrowValidationException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", categoryId, price: 100m, stock: 3);
        var command = new DecreaseProductStockCommand(productId, 5);
        
        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => 
            _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("Insufficient stock", exception.Message);
        Assert.Contains("Available: 3", exception.Message);
        Assert.Equal(3, product.Stock); // Stock no debe cambiar
    }
    
    [Fact]
    public async Task Handle_WithNonExistingProduct_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new DecreaseProductStockCommand(Guid.NewGuid(), 5);
        
        _productRepositoryMock.Setup(x => x.GetByIdAsync(command.ProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);
        
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            _handler.Handle(command, CancellationToken.None));
    }
    
    [Fact]
    public async Task Handle_WithInactiveProduct_ShouldThrowValidationException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", categoryId, price: 100m, stock: 10);
        product.Deactivate(); // Marcar como inactivo
        var command = new DecreaseProductStockCommand(productId, 5);
        
        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => 
            _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("Cannot decrease stock for inactive product", exception.Message);
    }
    
    [Fact]
    public async Task Handle_WithDeletedProduct_ShouldThrowValidationException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", categoryId, price: 100m, stock: 10);
        product.SoftDelete(); // Marcar como eliminado
        var command = new DecreaseProductStockCommand(productId, 5);
        
        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => 
            _handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("Cannot decrease stock for deleted product", exception.Message);
    }
    
    [Fact]
    public async Task Handle_WithNegativeQuantity_ShouldBeValidatedByValidator()
    {
        // Arrange
        var command = new DecreaseProductStockCommand(Guid.NewGuid(), -5);
        var validator = new DecreaseProductStockCommandValidator(_productRepositoryMock.Object);
        
        // Act
        var result = await validator.ValidateAsync(command);
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Quantity" && e.ErrorMessage.Contains("greater than 0"));
    }
    
    [Fact]
    public async Task Handle_WhenStockBecomesLow_ShouldLogWarning()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", categoryId, price: 100m, stock: 6);
        var command = new DecreaseProductStockCommand(productId, 3); // Dejará stock en 3 (bajo)
        
        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.True(result);
        Assert.True(product.IsLowStock(5));
        // Verificar que se llamó al log de advertencia
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("low on stock")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}