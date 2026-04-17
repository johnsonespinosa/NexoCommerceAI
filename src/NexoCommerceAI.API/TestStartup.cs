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

public class TestStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddOpenApi();
        services.AddHealthChecks();
        services.AddApplication();

        // Configure InMemory database for testing
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("ProductApiTests"));
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHealthChecks("/health/live");
            endpoints.MapHealthChecks("/health/ready");

            endpoints.MapGet("/api/products", async (ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetProductsQuery(), cancellationToken);
                return result.ToOk();
            });

            endpoints.MapGet("/api/products/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetProductByIdQuery(id), cancellationToken);
                return result.ToOk();
            });

            endpoints.MapPost("/api/products", async (CreateProductRequest request, ISender sender, CancellationToken cancellationToken) =>
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

            endpoints.MapPut("/api/products/{id:guid}", async (Guid id, UpdateProductRequest request, ISender sender, CancellationToken cancellationToken) =>
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

            endpoints.MapDelete("/api/products/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new DeleteProductCommand(id), cancellationToken);
                return result.ToNoContent();
            });
        });
    }
}