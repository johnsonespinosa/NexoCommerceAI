using NexoCommerceAI.Application.Abstractions;
using NexoCommerceAI.Application.Abstractions.Messaging;
using NexoCommerceAI.Application.Common;

namespace NexoCommerceAI.Application.Products.DeleteProduct;

public sealed class DeleteProductCommandHandler(IProductRepository productRepository)
    : ICommandHandler<DeleteProductCommand>
{
    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
        {
            return Result.Failure(ProductErrors.NotFound(request.Id));
        }

        product.SoftDelete();
        await productRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
