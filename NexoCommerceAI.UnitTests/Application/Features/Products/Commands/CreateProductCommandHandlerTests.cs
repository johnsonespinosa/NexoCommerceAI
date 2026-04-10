using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Commands;
using NexoCommerceAI.Application.Features.Products.Handlers;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Products.Commands;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        Mock<ILogger<CreateProductCommandHandler>> loggerMock = new();
        
        _handler = new CreateProductCommandHandler(
            _productRepositoryMock.Object,
            _categoryRepositoryMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateProduct_WhenValid()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new CreateProductCommand
        {
            Name = "Test Product",
            CategoryId = categoryId,
            Price = 100m,
            Stock = 10,
            Description = "Test Description",
            IsFeatured = false
        };
        
        var category = Category.Create("Electronics");

        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _productRepositoryMock.Setup(r => r.ExistBySkuAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _productRepositoryMock.Setup(r => r.ExistBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(command.Name, result.Name);
        Assert.Equal(command.Price, result.Price);
        Assert.Equal(command.Stock, result.Stock);
        Assert.Equal(category.Name, result.CategoryName);
        
        _productRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _productRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenCategoryNotFound()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "Test Product",
            CategoryId = Guid.NewGuid(),
            Price = 100m,
            Stock = 10
        };

        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(command.CategoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        _productRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenSkuAlreadyExists()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new CreateProductCommand
        {
            Name = "Test Product",
            CategoryId = categoryId,
            Price = 100m,
            Stock = 10,
            Sku = "EXISTING-SKU"
        };
        
        var category = Category.Create("Electronics");

        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _productRepositoryMock.Setup(r => r.ExistBySkuAsync(command.Sku, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));
        _productRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenSlugAlreadyExists()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new CreateProductCommand
        {
            Name = "Test Product",
            CategoryId = categoryId,
            Price = 100m,
            Stock = 10,
            Slug = "existing-slug"
        };
        
        var category = Category.Create("Electronics");

        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _productRepositoryMock.Setup(r => r.ExistBySkuAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _productRepositoryMock.Setup(r => r.ExistBySlugAsync(command.Slug, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));
        _productRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}