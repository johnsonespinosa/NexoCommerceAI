using FluentValidation.TestHelper;
using Moq;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Commands;
using NexoCommerceAI.Application.Features.Products.Validators;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Products.Validators;

public class UpdateProductPriceCommandValidatorTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly UpdateProductPriceCommandValidator _validator;
    
    public UpdateProductPriceCommandValidatorTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _validator = new UpdateProductPriceCommandValidator(_productRepositoryMock.Object);
    }
    
    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new UpdateProductPriceCommand 
        { 
            ProductId = productId, 
            NewPrice = 100m,
            NewCompareAtPrice = 150m
        };
        
        _productRepositoryMock.Setup(x => x.ExistsAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Fact]
    public void Validate_WithEmptyProductId_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateProductPriceCommand { ProductId = Guid.Empty, NewPrice = 100m };
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProductId);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    [InlineData(1000001)]
    public void Validate_WithInvalidPrice_ShouldHaveError(decimal price)
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new UpdateProductPriceCommand { ProductId = productId, NewPrice = price };
        
        _productRepositoryMock.Setup(x => x.ExistsAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPrice);
    }
    
    [Fact]
    public void Validate_WithPriceHavingMoreThan2Decimals_ShouldHaveError()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new UpdateProductPriceCommand { ProductId = productId, NewPrice = 100.999m };
        
        _productRepositoryMock.Setup(x => x.ExistsAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPrice)
            .WithErrorMessage("Price must have at most 2 decimal places");
    }
}