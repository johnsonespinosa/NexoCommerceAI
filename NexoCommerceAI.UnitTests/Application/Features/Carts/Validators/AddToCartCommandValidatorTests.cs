using FluentValidation.TestHelper;
using NexoCommerceAI.Application.Features.Carts.Commands;
using NexoCommerceAI.Application.Features.Carts.Validators;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Carts.Validators;

public class AddToCartCommandValidatorTests
{
    private readonly AddToCartCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldNotHaveErrors_WhenValid()
    {
        // Arrange
        var command = new AddToCartCommand(Guid.NewGuid(), Guid.NewGuid(), 2);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenUserIdIsEmpty()
    {
        // Arrange
        var command = new AddToCartCommand(Guid.Empty, Guid.NewGuid(), 2);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("UserId is required");
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenProductIdIsEmpty()
    {
        // Arrange
        var command = new AddToCartCommand(Guid.NewGuid(), Guid.Empty, 2);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProductId)
            .WithErrorMessage("ProductId is required");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Validate_ShouldHaveError_WhenQuantityIsInvalid(int quantity)
    {
        // Arrange
        var command = new AddToCartCommand(Guid.NewGuid(), Guid.NewGuid(), quantity);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Quantity)
            .WithErrorMessage("Quantity must be greater than 0");
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenQuantityExceedsMaximum()
    {
        // Arrange
        var command = new AddToCartCommand(Guid.NewGuid(), Guid.NewGuid(), 1000);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Quantity)
            .WithErrorMessage("Quantity cannot exceed 999");
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenSelectedPriceIsZero()
    {
        // Arrange
        var command = new AddToCartCommand(Guid.NewGuid(), Guid.NewGuid(), 2, 0);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SelectedPrice)
            .WithErrorMessage("Selected price must be greater than 0");
    }
}