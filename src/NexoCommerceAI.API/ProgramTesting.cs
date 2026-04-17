using MediatR;
using Microsoft.EntityFrameworkCore;
using NexoCommerceAI.API.Contracts.Products;
using NexoCommerceAI.API.Extensions;
using NexoCommerceAI.Application;
using NexoCommerceAI.Application.Abstractions;
using NexoCommerceAI.Application.Products.CreateProduct;
using NexoCommerceAI.Application.Products.DeleteProduct;
using NexoCommerceAI.Application.Products.GetProductById;
using NexoCommerceAI.Application.Products.GetProducts;
using NexoCommerceAI.Application.Products.UpdateProduct;
using NexoCommerceAI.Infrastructure.Persistence;
using NexoCommerceAI.Infrastructure.Persistence.Repositories;

namespace NexoCommerceAI.API;

public class ProgramTesting
{
    public static WebApplication CreateWebApplication(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddOpenApi();
        builder.Services.AddHealthChecks();
        builder.Services.AddApplication();

        // Configure InMemory database for testing
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("ProductApiTests"));
        builder.Services.AddScoped<IProductRepository, ProductRepository>();
        builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        app.MapHealthChecks("/health/live");
        app.MapHealthChecks("/health/ready");

        app.MapGet("/api/products", async (ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetProductsQuery(), cancellationToken);
            return result.ToOk();
        });

        app.MapGet("/api/products/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetProductByIdQuery(id), cancellationToken);
            return result.ToOk();
        });

        app.MapPost("/api/products", async (CreateProductRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new CreateProductCommand(
                Name: request.Name,
                CategoryId: request.CategoryId,
                Description: request.Description,
                Price: request.Price,
                CompareAtPrice: request.CompareAtPrice,
                Sku: request.Sku,
                Stock: request.Stock,
                IsFeatured: request.IsFeatured), cancellationToken);

            return result.ToCreated(value => $"/api/products/{value.Id}");
        });

        app.MapPut("/api/products/{id:guid}", async (Guid id, UpdateProductRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
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
        });

        app.MapDelete("/api/products/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new DeleteProductCommand(id), cancellationToken);
            return result.ToNoContent();
        });

        return app;
    }
}