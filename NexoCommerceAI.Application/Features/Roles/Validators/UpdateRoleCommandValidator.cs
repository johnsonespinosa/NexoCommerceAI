using FluentValidation;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Roles.Commands;

namespace NexoCommerceAI.Application.Features.Roles.Validators;

public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator(IRoleRepository roleRepository)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Role ID is required");
        
        RuleFor(x => x.Name)
            .MaximumLength(50).When(x => x.Name != null)
            .WithMessage("Role name must not exceed 50 characters")
            .Matches(@"^[a-zA-Z]+$").When(x => x.Name != null)
            .WithMessage("Role name can only contain letters")
            .MustAsync(async (command, name, cancellation) =>
                string.IsNullOrWhiteSpace(name) ||
                await roleRepository.IsRoleNameUniqueAsync(name, command.Id, cancellation))
            .WithMessage("Role name already exists");
        
        RuleFor(x => x.Description)
            .MaximumLength(200).When(x => x.Description != null)
            .WithMessage("Role description must not exceed 200 characters");
    }
}