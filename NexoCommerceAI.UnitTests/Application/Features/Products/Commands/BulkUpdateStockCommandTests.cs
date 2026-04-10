using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Commands;
using NexoCommerceAI.Application.Features.Products.Handlers;
using NexoCommerceAI.Application.Features.Products.Models;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Products.Commands;

public class BulkUpdateStockCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ILogger<BulkUpdateStockCommandHandler>> _loggerMock;
    private readonly BulkUpdateStockCommandHandler _handler;

    public BulkUpdateStockCommandHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _loggerMock = new Mock<ILogger<BulkUpdateStockCommandHandler>>();
        
        _handler = new BulkUpdateStockCommandHandler(_productRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateAllProducts_WhenAllItemsAreValid()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var product1 = Product.Create("Product 1", categoryId, price: 100m, stock: 10);
        var product2 = Product.Create("Product 2", categoryId, price: 200m, stock: 20);
        
        var items = new List<BulkStockUpdateItem>
        {
            new() { ProductId = product1.Id, NewStock = 15 },
            new() { ProductId = product2.Id, NewStock = 25 }
        };
        
        var command = new BulkUpdateStockCommand(items);
        
        _productRepositoryMock.Setup(r => r.GetByIdAsync(product1.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product1);
        _productRepositoryMock.Setup(r => r.GetByIdAsync(product2.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product2);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.TotalItems);
        Assert.Equal(2, result.SuccessCount);
        Assert.Equal(0, result.FailureCount);
        Assert.True(result.AllSucceeded);
        Assert.Equal(15, product1.Stock);
        Assert.Equal(25, product2.Stock);
        
        _productRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _productRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSkipNonExistingProducts_AndReturnErrors()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var existingProduct = Product.Create("Existing Product", categoryId, price: 100m, stock: 10);
        var nonExistingId = Guid.NewGuid();
        
        var items = new List<BulkStockUpdateItem>
        {
            new() { ProductId = existingProduct.Id, NewStock = 15 },
            new() { ProductId = nonExistingId, NewStock = 25 }
        };
        
        var command = new BulkUpdateStockCommand(items);
        
        _productRepositoryMock.Setup(r => r.GetByIdAsync(existingProduct.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);
        _productRepositoryMock.Setup(r => r.GetByIdAsync(nonExistingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.TotalItems);
        Assert.Equal(1, result.SuccessCount);
        Assert.Equal(1, result.FailureCount);
        Assert.True(result.PartialSuccess);
        Assert.Single(result.Errors);
        Assert.Equal(nonExistingId, result.Errors.First().ProductId);
        Assert.Equal("Product not found or deleted", result.Errors.First().ErrorMessage);
        
        _productRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _productRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSkipInactiveProducts_AndReturnErrors()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var activeProduct = Product.Create("Active Product", categoryId, price: 100m, stock: 10);
        var inactiveProduct = Product.Create("Inactive Product", categoryId, price: 200m, stock: 20);
        inactiveProduct.Deactivate();
        
        var items = new List<BulkStockUpdateItem>
        {
            new() { ProductId = activeProduct.Id, NewStock = 15 },
            new() { ProductId = inactiveProduct.Id, NewStock = 25 }
        };
        
        var command = new BulkUpdateStockCommand(items);
        
        _productRepositoryMock.Setup(r => r.GetByIdAsync(activeProduct.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeProduct);
        _productRepositoryMock.Setup(r => r.GetByIdAsync(inactiveProduct.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inactiveProduct);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.TotalItems);
        Assert.Equal(1, result.SuccessCount);
        Assert.Equal(1, result.FailureCount);
        Assert.True(result.PartialSuccess);
        Assert.Single(result.Errors);
        Assert.Equal(inactiveProduct.Id, result.Errors.First().ProductId);
        Assert.Contains("inactive", result.Errors.First().ErrorMessage.ToLower());
        
        Assert.Equal(15, activeProduct.Stock);
        Assert.Equal(20, inactiveProduct.Stock); // No cambió
    }

    [Fact]
    public async Task Handle_ShouldSkipDeletedProducts_AndReturnErrors()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var activeProduct = Product.Create("Active Product", categoryId, price: 100m, stock: 10);
        var deletedProduct = Product.Create("Deleted Product", categoryId, price: 200m, stock: 20);
        deletedProduct.SoftDelete();
        
        var items = new List<BulkStockUpdateItem>
        {
            new() { ProductId = activeProduct.Id, NewStock = 15 },
            new() { ProductId = deletedProduct.Id, NewStock = 25 }
        };
        
        var command = new BulkUpdateStockCommand(items);
        
        _productRepositoryMock.Setup(r => r.GetByIdAsync(activeProduct.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeProduct);
        _productRepositoryMock.Setup(r => r.GetByIdAsync(deletedProduct.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null); // Repositorio no retorna productos eliminados

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.TotalItems);
        Assert.Equal(1, result.SuccessCount);
        Assert.Equal(1, result.FailureCount);
        Assert.Single(result.Errors);
        Assert.Equal(deletedProduct.Id, result.Errors.First().ProductId);
    }

    [Fact]
    public async Task Handle_ShouldHandleMixedResults_WhenSomeProductsHaveErrors()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var product1 = Product.Create("Product 1", categoryId, price: 100m, stock: 10);
        var product2 = Product.Create("Product 2", categoryId, price: 200m, stock: 20);
        var product3 = Product.Create("Product 3", categoryId, price: 300m, stock: 30);
        
        var items = new List<BulkStockUpdateItem>
        {
            new() { ProductId = product1.Id, NewStock = 15 },
            new() { ProductId = product2.Id, NewStock = -5 }, // Stock negativo - será rechazado por validación
            new() { ProductId = product3.Id, NewStock = 35 }
        };
        
        var command = new BulkUpdateStockCommand(items);
        
        _productRepositoryMock.Setup(r => r.GetByIdAsync(product1.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product1);
        _productRepositoryMock.Setup(r => r.GetByIdAsync(product2.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product2);
        _productRepositoryMock.Setup(r => r.GetByIdAsync(product3.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product3);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(3, result.TotalItems);
        Assert.Equal(2, result.SuccessCount);
        Assert.Equal(1, result.FailureCount);
        Assert.True(result.PartialSuccess);
        Assert.Equal(15, product1.Stock);
        Assert.Equal(20, product2.Stock); // No cambió
        Assert.Equal(35, product3.Stock);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllFailed_WhenAllProductsHaveErrors()
    {
        // Arrange
        var nonExistingId1 = Guid.NewGuid();
        var nonExistingId2 = Guid.NewGuid();
        
        var items = new List<BulkStockUpdateItem>
        {
            new() { ProductId = nonExistingId1, NewStock = 15 },
            new() { ProductId = nonExistingId2, NewStock = 25 }
        };
        
        var command = new BulkUpdateStockCommand(items);
        
        _productRepositoryMock.Setup(r => r.GetByIdAsync(nonExistingId1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);
        _productRepositoryMock.Setup(r => r.GetByIdAsync(nonExistingId2, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.TotalItems);
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(2, result.FailureCount);
        Assert.True(result.AllFailed);
        Assert.Equal(2, result.Errors.Count);
        
        _productRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        _productRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldNotSaveChanges_WhenNoSuccessItems()
    {
        // Arrange
        var items = new List<BulkStockUpdateItem>
        {
            new() { ProductId = Guid.NewGuid(), NewStock = 15 },
            new() { ProductId = Guid.NewGuid(), NewStock = 25 }
        };
        
        var command = new BulkUpdateStockCommand(items);
        
        _productRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(2, result.FailureCount);
        _productRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldUpdateStockAndTriggerLowStockWarning_WhenStockBecomesLow()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", categoryId, price: 100m, stock: 10);
        
        var items = new List<BulkStockUpdateItem>
        {
            new() { ProductId = product.Id, NewStock = 3 } // Stock bajo (<=5)
        };
        
        var command = new BulkUpdateStockCommand(items);
        
        _productRepositoryMock.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(1, result.SuccessCount);
        Assert.Equal(3, product.Stock);
        Assert.True(product.IsLowStock());
        
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

    [Fact]
    public async Task Handle_ShouldProcessLargeBatchEfficiently()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var items = new List<BulkStockUpdateItem>();
        var products = new List<Product>();
        
        // Crear 50 productos
        for (int i = 0; i < 50; i++)
        {
            var product = Product.Create($"Product {i}", categoryId, price: 100m, stock: 10);
            products.Add(product);
            items.Add(new BulkStockUpdateItem { ProductId = product.Id, NewStock = i + 5 });
        }
        
        var command = new BulkUpdateStockCommand(items);
        
        foreach (var product in products)
        {
            _productRepositoryMock.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);
        }

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(50, result.TotalItems);
        Assert.Equal(50, result.SuccessCount);
        Assert.Equal(0, result.FailureCount);
        
        // Verificar que cada producto fue actualizado
        for (int i = 0; i < 50; i++)
        {
            Assert.Equal(i + 5, products[i].Stock);
        }
        
        _productRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Exactly(50));
        _productRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldIncludeErrorDetails_WhenExceptionOccurs()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var product = Product.Create("Test Product", categoryId, price: 100m, stock: 10);
        
        var items = new List<BulkStockUpdateItem>
        {
            new() { ProductId = product.Id, NewStock = 15 }
        };
        
        var command = new BulkUpdateStockCommand(items);
        
        _productRepositoryMock.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _productRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(1, result.TotalItems);
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(1, result.FailureCount);
        Assert.Single(result.Errors);
        Assert.Equal(product.Id, result.Errors.First().ProductId);
        Assert.Equal("Database error", result.Errors.First().ErrorMessage);
        Assert.Equal(15, result.Errors.First().AttemptedStock);
    }
}