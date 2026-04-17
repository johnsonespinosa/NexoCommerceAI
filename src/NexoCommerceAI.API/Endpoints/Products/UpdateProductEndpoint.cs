using MediatR;
using NexoCommerceAI.API.Common;
using NexoCommerceAI.API.Contracts.Products;
using NexoCommerceAI.API.Extensions;
using NexoCommerceAI.Application.Products.UpdateProduct;

namespace NexoCommerceAI.API.Endpoints.Products;

internal sealed class UpdateProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/products/{id:guid}", async (Guid id, UpdateProductRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var validation = RequestValidation.Validate(request);
            if (validation is not null) return validation;

            var result = await sender.Send(new UpdateProductCommand(
                Id: id,
                Name: request.Name,
                CategoryId: request.CategoryId,
                Description: request.Description,
                Price: request.Price,
                CompareAtPrice: request.CompareAtPrice,
                Sku: request.Sku,
                Stock: request.Stock,
                IsFeatured: request.IsFeatured), cancellationToken);

            return result.ToOk();
        })
        .WithTags(Tags.Products);
    }
}
