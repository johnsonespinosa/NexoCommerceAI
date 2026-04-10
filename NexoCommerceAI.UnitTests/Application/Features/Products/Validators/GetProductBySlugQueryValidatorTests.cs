using System;
using FluentValidation.TestHelper;
using NexoCommerceAI.Application.Features.Products.Queries;
using NexoCommerceAI.Application.Features.Products.Validators;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Products.Validators;

public class GetProductBySlugQueryValidatorTests
{
    private readonly GetProductBySlugQueryValidator _validator;
    
    public GetProductBySlugQueryValidatorTests()
    {
        _validator = new GetProductBySlugQueryValidator();
    }
    
    [Fact]
    public void Validate_WithValidSlug_ShouldNotHaveError()
    {
        // Arrange
        var query = new GetProductBySlugQuery("valid-slug-123");
        
        // Act
        var result = _validator.TestValidate(query);
        
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Slug);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithEmptySlug_ShouldHaveError(string slug)
    {
        // Arrange
        var query = new GetProductBySlugQuery(slug);
        
        // Act
        var result = _validator.TestValidate(query);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Slug)
            .WithErrorMessage("Slug is required");
    }
    
    [Theory]
    [InlineData("Invalid Slug")]
    [InlineData("UPPERCASE")]
    [InlineData("invalid__slug")]
    [InlineData("invalid--slug")]
    [InlineData("slug-with-$pecial")]
    public void Validate_WithInvalidFormatSlug_ShouldHaveError(string slug)
    {
        // Arrange
        var query = new GetProductBySlugQuery(slug);
        
        // Act
        var result = _validator.TestValidate(query);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Slug)
            .WithErrorMessage("Slug must contain only lowercase letters, numbers, and hyphens");
    }
    
    [Fact]
    public void Validate_WithVeryLongSlug_ShouldHaveError()
    {
        // Arrange
        var longSlug = new string('a', 201);
        var query = new GetProductBySlugQuery(longSlug);
        
        // Act
        var result = _validator.TestValidate(query);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Slug)
            .WithErrorMessage("Slug must not exceed 200 characters");
    }
}
