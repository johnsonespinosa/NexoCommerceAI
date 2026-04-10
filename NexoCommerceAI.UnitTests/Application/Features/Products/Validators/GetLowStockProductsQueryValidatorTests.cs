using FluentValidation.TestHelper;
using NexoCommerceAI.Application.Features.Products.Queries;
using NexoCommerceAI.Application.Features.Products.Validators;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Products.Validators;

public class GetLowStockProductsQueryValidatorTests
{
    private readonly GetLowStockProductsQueryValidator _validator = new();

    [Fact]
    public void Validate_WithValidThreshold_ShouldNotHaveError()
    {
        // Arrange
        var query = new GetLowStockProductsQuery(5);
        
        // Act
        var result = _validator.TestValidate(query);
        
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Threshold);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Validate_WithThresholdLessThanOrEqualZero_ShouldHaveError(int threshold)
    {
        // Arrange
        var query = new GetLowStockProductsQuery(threshold);
        
        // Act
        var result = _validator.TestValidate(query);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Threshold)
            .WithErrorMessage("Threshold must be greater than 0");
    }
    
    [Fact]
    public void Validate_WithThresholdExceedingMax_ShouldHaveError()
    {
        // Arrange
        var query = new GetLowStockProductsQuery(101);
        
        // Act
        var result = _validator.TestValidate(query);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Threshold)
            .WithErrorMessage("Threshold cannot exceed 100");
    }
}