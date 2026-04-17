using MediatR;
using NexoCommerceAI.API.Extensions;
using NexoCommerceAI.Application.Products.GetProducts;

namespace NexoCommerceAI.API.Endpoints.Products;

internal sealed class GetProductsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/products", async (
            ISender sender,
            int page = 1,
            int pageSize = 20,
            string? search = null,
            Guid? categoryId = null,
            string? sku = null,
            bool? isFeatured = null,
            CancellationToken cancellationToken = default) =>
        {
            var result = await sender.Send(new GetProductsQuery(page, pageSize, search, categoryId, sku, isFeatured), cancellationToken);
            return result.ToOk();
        })
        .WithTags(Tags.Products);
    }
}
