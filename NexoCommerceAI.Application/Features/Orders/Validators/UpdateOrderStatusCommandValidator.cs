using FluentValidation;
using NexoCommerceAI.Application.Features.Orders.Commands;
using NexoCommerceAI.Domain.Enums;

namespace NexoCommerceAI.Application.Features.Orders.Validators;

public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId is required");
        
        RuleFor(x => x.NewStatus)
            .NotEmpty().WithMessage("NewStatus is required")
            .Must(BeValidStatus).WithMessage("Invalid order status");
    }
    
    private static bool BeValidStatus(string status)
    {
        return Enum.TryParse<OrderStatus>(status, true, out _);
    }
}