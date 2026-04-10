using FluentValidation;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Products.Commands;

namespace NexoCommerceAI.Application.Features.Products.Validators;

public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator(IProductRepository productRepository)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Product ID is required")
            .MustAsync(async (id, cancellation) => 
                await productRepository.ExistsAsync(id, cancellation))
            .WithMessage("Product does not exist");
        
        // Validación adicional: No se puede eliminar un producto que ya está eliminado
        RuleFor(x => x.Id)
            .MustAsync(async (id, cancellation) =>
            {
                var product = await productRepository.GetByIdAsync(id, cancellation);
                return product != null && !product.IsDeleted;
            })
            .WithMessage("Product is already deleted");
        
        // Validación opcional: No se puede eliminar un producto con stock positivo (regla de negocio)
        RuleFor(x => x.Id)
            .MustAsync(async (id, cancellation) =>
            {
                var product = await productRepository.GetByIdAsync(id, cancellation);
                return product == null || product.Stock == 0;
            })
            .WithMessage("Cannot delete product with positive stock. Please reduce stock to zero first.");
    }
}