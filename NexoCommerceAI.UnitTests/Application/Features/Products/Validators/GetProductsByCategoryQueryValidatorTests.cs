using FluentValidation.TestHelper;
using Moq;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Queries;
using NexoCommerceAI.Application.Features.Products.Validators;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Products.Validators;

public class GetProductsByCategoryQueryValidatorTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly GetProductsByCategoryQueryValidator _validator;
    
    public GetProductsByCategoryQueryValidatorTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _validator = new GetProductsByCategoryQueryValidator(_categoryRepositoryMock.Object);
    }
    
    [Fact]
    public void Validate_WithValidQuery_ShouldNotHaveErrors()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var query = new GetProductsByCategoryQuery(categoryId, 10);
        
        _categoryRepositoryMock.Setup(x => x.ExistsAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        // Act
        var result = _validator.TestValidate(query);
        
        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Fact]
    public void Validate_WithEmptyCategoryId_ShouldHaveError()
    {
        // Arrange
        var query = new GetProductsByCategoryQuery(Guid.Empty, 10);
        
        // Act
        var result = _validator.TestValidate(query);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CategoryId)
            .WithErrorMessage("Category ID is required");
    }
    
    [Fact]
    public void Validate_WithNonExistingCategory_ShouldHaveError()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var query = new GetProductsByCategoryQuery(categoryId, 10);
        
        _categoryRepositoryMock.Setup(x => x.ExistsAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        
        // Act
        var result = _validator.TestValidate(query);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CategoryId)
            .WithErrorMessage("Category does not exist");
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Validate_WithInvalidTake_ShouldHaveError(int take)
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var query = new GetProductsByCategoryQuery(categoryId, take);
        
        _categoryRepositoryMock.Setup(x => x.ExistsAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
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
        var categoryId = Guid.NewGuid();
        var query = new GetProductsByCategoryQuery(categoryId, 101);
        
        _categoryRepositoryMock.Setup(x => x.ExistsAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        // Act
        var result = _validator.TestValidate(query);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Take)
            .WithErrorMessage("Take cannot exceed 100");
    }
}