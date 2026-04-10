using FluentValidation;
using NexoCommerceAI.Application.Features.Auth.Commands;

namespace NexoCommerceAI.Application.Features.Auth.Validators;

public class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
        
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Username is required")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters")
            .MaximumLength(50).WithMessage("Username must not exceed 50 characters")
            .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers, and underscores");
        
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters");
    }
}