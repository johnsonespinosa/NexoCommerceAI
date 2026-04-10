using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Commands;
using NexoCommerceAI.Application.Features.Products.Handlers;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Products.Commands;

public class UpdateProductPriceCommandTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly UpdateProductPriceCommandHandler _handler;
    
    public UpdateProductPriceCommandTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        Mock<ILogger<UpdateProductPriceCommandHandler>> loggerMock = new();
        _handler = new UpdateProductPriceCommandHandler(_productRepositoryMock.Object, loggerMock.Object);
    }
    
    [Fact]
    public async Task Handle_WithValidPrice_ShouldUpdatePrice()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", categoryId, price: 100m, stock: 10);
        var command = new UpdateProductPriceCommand 
        { 
            ProductId = productId, 
            NewPrice = 150m,
            NewCompareAtPrice = null
        };
        
        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.True(result);
        Assert.Equal(150m, product.Price);
        Assert.Null(product.CompareAtPrice);
        _productRepositoryMock.Verify(x => x.UpdateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
        _productRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_WithValidPriceAndCompareAtPrice_ShouldUpdateBoth()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", categoryId, price: 100m, stock: 10);
        var command = new UpdateProductPriceCommand 
        { 
            ProductId = productId, 
            NewPrice = 120m,
            NewCompareAtPrice = 150m
        };
        
        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.True(result);
        Assert.Equal(120m, product.Price);
        Assert.Equal(150m, product.CompareAtPrice);
    }
    
    [Fact]
    public async Task Handle_WithNonExistingProduct_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new UpdateProductPriceCommand { ProductId = Guid.NewGuid(), NewPrice = 150m };
        
        _productRepositoryMock.Setup(x => x.GetByIdAsync(command.ProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);
        
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            _handler.Handle(command, CancellationToken.None));
    }
    
    [Fact]
    public async Task Handle_WithInvalidPriceZero_ShouldThrowValidationException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", categoryId, price: 100m, stock: 10);
        var command = new UpdateProductPriceCommand { ProductId = productId, NewPrice = 0 };
        
        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => 
            _handler.Handle(command, CancellationToken.None));
    }
    
    [Fact]
    public async Task Handle_WithInvalidCompareAtPriceLessThanPrice_ShouldThrowValidationException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", categoryId, price: 100m, stock: 10);
        var command = new UpdateProductPriceCommand 
        { 
            ProductId = productId, 
            NewPrice = 150m,
            NewCompareAtPrice = 120m
        };
        
        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => 
            _handler.Handle(command, CancellationToken.None));
    }
    
    [Fact]
    public async Task Handle_WithCompareAtPriceMoreThanDouble_ShouldThrowValidationException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", categoryId, price: 100m, stock: 10);
        var command = new UpdateProductPriceCommand 
        { 
            ProductId = productId, 
            NewPrice = 100m,
            NewCompareAtPrice = 250m  // 2.5 veces el precio
        };
        
        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => 
            _handler.Handle(command, CancellationToken.None));
    }
    
    [Fact]
    public async Task Handle_WithCompareAtPriceLessThan10PercentHigher_ShouldThrowValidationException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", categoryId, price: 100m, stock: 10);
        var command = new UpdateProductPriceCommand 
        { 
            ProductId = productId, 
            NewPrice = 100m,
            NewCompareAtPrice = 105m  // Solo 5% más alto
        };
        
        _productRepositoryMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => 
            _handler.Handle(command, CancellationToken.None));
    }
}