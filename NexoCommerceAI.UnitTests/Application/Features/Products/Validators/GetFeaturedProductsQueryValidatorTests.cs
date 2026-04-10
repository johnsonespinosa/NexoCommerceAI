// Tests/Application/Features/Products/Queries/GetFeaturedProducts/GetFeaturedProductsQueryValidatorTests.cs

using FluentValidation.TestHelper;
using NexoCommerceAI.Application.Features.Products.Queries;
using NexoCommerceAI.Application.Features.Products.Validators;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Products.Validators;

public class GetFeaturedProductsQueryValidatorTests
{
    private readonly GetFeaturedProductsQueryValidator _validator = new();

    [Fact]
    public void Validate_WithValidTake_ShouldNotHaveError()
    {
        // Arrange
        var query = new GetFeaturedProductsQuery(5);
        
        // Act
        var result = _validator.TestValidate(query);
        
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Take);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Validate_WithInvalidTakeLessThanOrEqualZero_ShouldHaveError(int take)
    {
        // Arrange
        var query = new GetFeaturedProductsQuery(take);
        
        // Act
        var result = _validator.TestValidate(query);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Take)
            .WithErrorMessage("Take must be greater than 0");
    }
    
    [Fact]
    public void Validate_WithTakeExceedingMax_ShouldHaveError()
    {
        // Arrange
        var query = new GetFeaturedProductsQuery(101);
        
        // Act
        var result = _validator.TestValidate(query);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Take)
            .WithErrorMessage("Take cannot exceed 100");
    }
}