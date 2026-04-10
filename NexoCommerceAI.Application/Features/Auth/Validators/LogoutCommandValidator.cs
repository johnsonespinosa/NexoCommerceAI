using FluentValidation;
using NexoCommerceAI.Application.Features.Auth.Commands;

namespace NexoCommerceAI.Application.Features.Auth.Validators;

public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}