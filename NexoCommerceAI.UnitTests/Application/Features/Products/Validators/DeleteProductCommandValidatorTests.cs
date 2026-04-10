using FluentValidation.TestHelper;
using Moq;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Commands;
using NexoCommerceAI.Application.Features.Products.Validators;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Products.Validators;

public class DeleteProductCommandValidatorTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly DeleteProductCommandValidator _validator;
    
    public DeleteProductCommandValidatorTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _validator = new DeleteProductCommandValidator(_productRepositoryMock.Object);
    }
    
    [Fact]
    public void Validate_WithEmptyId_ShouldHaveError()
    {
        // Arrange
        var command = new DeleteProductCommand(Guid.Empty);
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }
    
    [Fact]
    public void Validate_WithNonExistentProduct_ShouldHaveError()
    {
        // Arrange
        var command = new DeleteProductCommand(Guid.NewGuid());
        _productRepositoryMock.Setup(x => x.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        
        // Act
        var result = _validator.TestValidate(command);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Product does not exist");
    }
}