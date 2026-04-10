using FluentValidation.TestHelper;
using NexoCommerceAI.Application.Features.Products.Queries;
using NexoCommerceAI.Application.Features.Products.Validators;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Products.Validators;

public class GetProductByIdQueryValidatorTests
{
    private readonly GetProductByIdQueryValidator _validator = new();

    [Fact]
    public void Validate_WithValidId_ShouldNotHaveError()
    {
        // Arrange
        var query = new GetProductByIdQuery(Guid.NewGuid());
        
        // Act
        var result = _validator.TestValidate(query);
        
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }
    
    [Fact]
    public void Validate_WithEmptyId_ShouldHaveError()
    {
        // Arrange
        var query = new GetProductByIdQuery(Guid.Empty);
        
        // Act
        var result = _validator.TestValidate(query);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Product ID is required");
    }
}