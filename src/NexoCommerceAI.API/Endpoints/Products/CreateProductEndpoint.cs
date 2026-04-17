using MediatR;
using NexoCommerceAI.API.Common;
using NexoCommerceAI.API.Contracts.Products;
using NexoCommerceAI.API.Extensions;
using NexoCommerceAI.Application.Products.CreateProduct;

namespace NexoCommerceAI.API.Endpoints.Products;

internal sealed class CreateProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/products", (CreateProductRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var validation = RequestValidation.Validate(request);
            if (validation is not null) return Task.FromResult<IResult>(validation);

            return sender.Send(new CreateProductCommand(
                Name: request.Name,
                CategoryId: request.CategoryId,
                Description: request.Description,
                Price: request.Price,
                CompareAtPrice: request.CompareAtPrice,
                Sku: request.Sku,
                Stock: request.Stock,
                IsFeatured: request.IsFeatured), cancellationToken)
                .ContinueWith<IResult>(t =>
                {
                    if (t.IsCanceled) return Results.Problem();
                    if (t.IsFaulted) return Results.Problem(detail: t.Exception?.GetBaseException().Message);
                    return t.Result.ToCreated(value => $"/api/products/{value.Id}");
                });
        })
        .WithTags(Tags.Products);
    }
}
