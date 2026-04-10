using FluentValidation.TestHelper;
using Moq;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Commands;
using NexoCommerceAI.Application.Features.Products.Validators;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Products.Validators;

public class DecreaseProductStockCommandValidatorTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly DecreaseProductStockCommandValidator _validator;
    
    public DecreaseProductStockCommandValidatorTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _validator = new DecreaseProductStockCommandValidator(_productRepositoryMock.Object);
    }
    
    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new DecreaseProductStockCommand(productId, 5);
        
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
        var command = new DecreaseProductStockCommand(Guid.Empty, 5);
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProductId);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Validate_WithInvalidQuantity_ShouldHaveError(int quantity)
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new DecreaseProductStockCommand(productId, quantity);
        
        _productRepositoryMock.Setup(x => x.ExistsAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Quantity)
            .WithErrorMessage("Quantity must be greater than 0");
    }
}