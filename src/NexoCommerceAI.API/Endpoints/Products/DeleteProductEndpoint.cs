using MediatR;
using NexoCommerceAI.API.Extensions;
using NexoCommerceAI.Application.Products.DeleteProduct;

namespace NexoCommerceAI.API.Endpoints.Products;

internal sealed class DeleteProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/products/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new DeleteProductCommand(id), cancellationToken);
            return result.ToNoContent();
        })
        .WithTags(Tags.Products);
    }
}
