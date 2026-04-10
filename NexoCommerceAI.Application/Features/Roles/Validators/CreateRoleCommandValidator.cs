using FluentValidation;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Roles.Commands;

namespace NexoCommerceAI.Application.Features.Roles.Validators;

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator(IRoleRepository roleRepository)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required")
            .MaximumLength(50).WithMessage("Role name must not exceed 50 characters")
            .Matches(@"^[a-zA-Z]+$").WithMessage("Role name can only contain letters")
            .MustAsync(async (name, cancellation) => 
                await roleRepository.IsRoleNameUniqueAsync(name, cancellation))
            .WithMessage("Role name already exists");
        
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Role description is required")
            .MaximumLength(200).WithMessage("Role description must not exceed 200 characters");
    }
}