using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Handlers;
using NexoCommerceAI.Application.Features.Products.Queries;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Products.Queries;

public class GetProductsOnSaleQueryHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly GetProductsOnSaleQueryHandler _handler;
    
    public GetProductsOnSaleQueryHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        Mock<ILogger<GetProductsOnSaleQueryHandler>> loggerMock = new();
        _handler = new GetProductsOnSaleQueryHandler(_productRepositoryMock.Object, loggerMock.Object);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnAllProductsOnSale_WhenTakeIsNull()
    {
        // Arrange
        var query = new GetProductsOnSaleQuery();
        var category = Category.Create("Electronics");
        var expectedProducts = new List<Product>
        {
            Product.Create("Product 1", category.Id, price: 80m, compareAtPrice: 100m, stock: 10),
            Product.Create("Product 2", category.Id, price: 90m, compareAtPrice: 120m, stock: 20),
            Product.Create("Product 3", category.Id, price: 70m, compareAtPrice: 90m, stock: 5)
        };
        
        _productRepositoryMock.Setup(x => x.GetProductsOnSaleAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProducts);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.Equal(3, result.Count);
        _productRepositoryMock.Verify(x => x.GetProductsOnSaleAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_ShouldLimitResults_WhenTakeIsSpecified()
    {
        // Arrange
        var take = 2;
        var query = new GetProductsOnSaleQuery(take);
        var category = Category.Create("Electronics");
        var allProducts = new List<Product>
        {
            Product.Create("Product 1", category.Id, price: 80m, compareAtPrice: 100m, stock: 10),
            Product.Create("Product 2", category.Id, price: 90m, compareAtPrice: 120m, stock: 20),
            Product.Create("Product 3", category.Id, price: 70m, compareAtPrice: 90m, stock: 5)
        };
        
        _productRepositoryMock.Setup(x => x.GetProductsOnSaleAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(allProducts);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.Equal(take, result.Count);
    }
    
    [Fact]
    public async Task Handle_WhenNoProductsOnSale_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetProductsOnSaleQuery();
        
        _productRepositoryMock.Setup(x => x.GetProductsOnSaleAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    
    [Fact]
    public async Task Handle_ShouldMapToProductResponseCorrectly()
    {
        // Arrange
        var query = new GetProductsOnSaleQuery();
        var category = Category.Create("Electronics", "electronics");
        var product = Product.Create("Test Product", category.Id, price: 80m, compareAtPrice: 100m, stock: 10);
        var expectedProducts = new List<Product> { product };
        
        _productRepositoryMock.Setup(x => x.GetProductsOnSaleAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProducts);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        // Assert
        var response = result[0];
        Assert.Equal(product.Id, response.Id);
        Assert.Equal(product.Name, response.Name);
        Assert.Equal(product.Price, response.Price);
        Assert.Equal(product.CompareAtPrice, response.CompareAtPrice);
        Assert.Equal(product.IsOnSale(), response.IsOnSale);
        Assert.Equal(product.GetDiscountPercentage(), response.DiscountPercentage);
        Assert.Equal(category.Id, response.CategoryId);
        Assert.Equal(category.Name, response.CategoryName);
    }
}