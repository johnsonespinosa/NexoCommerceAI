using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexoCommerceAI.Application.Common.Models;
using NexoCommerceAI.Application.Features.Products.Commands;
using NexoCommerceAI.Application.Features.Products.Models;
using NexoCommerceAI.Infrastructure.Data;
using NexoCommerceAI.IntegrationTests.Helpers;
using Xunit;

namespace NexoCommerceAI.IntegrationTests.Api.Controllers;

public class ProductsControllerTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetProducts_ShouldReturnPaginatedResult()
    {
        // Act
        var response = await _client.GetAsync("/api/products?pageNumber=1&pageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PaginatedResult<ProductResponse>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.True(result.Items.Count > 0);
    }

    [Fact]
    public async Task GetProductById_ShouldReturnProduct_WhenExists()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var product = await dbContext.Products.FirstAsync();

        // Act
        var response = await _client.GetAsync($"/api/products/{product.Id}");
        var result = await response.Content.ReadFromJsonAsync<ProductResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(product.Id, result.Id);
    }

    [Fact]
    public async Task GetProductById_ShouldReturnNotFound_WhenNotExists()
    {
        // Act
        var response = await _client.GetAsync($"/api/products/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_ShouldCreateNewProduct_WhenValid()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var category = await dbContext.Categories.FirstAsync();

        var command = new CreateProductCommand
        (
            Name: "Integration Test Product",
            CategoryId: category.Id,
            Price: 199.99m,
            Stock: 15,
            Description: "Test Description"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", command);
        var result = await response.Content.ReadFromJsonAsync<ProductResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(command.Name, result.Name);
        Assert.Equal(command.Price, result.Price);
    }

    [Fact]
    public async Task UpdateProduct_ShouldUpdateExistingProduct_WhenValid()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var product = await dbContext.Products.FirstAsync();

        var command = new UpdateProductCommand
        (
            Id: product.Id,
            Name: "Updated Product Name",
            Price: 299.99m
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/products/{product.Id}", command);
        var result = await response.Content.ReadFromJsonAsync<ProductResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(command.Name, result.Name);
        Assert.Equal(command.Price, result.Price);
    }

    [Fact]
    public async Task DeleteProduct_ShouldSoftDeleteProduct_WhenValid()
    {
        // Arrange
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var product = await dbContext.Products.FirstAsync(p => p.Stock == 0);

        // Act
        var response = await _client.DeleteAsync($"/api/products/{product.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        // Verificar soft delete
        var deletedProduct = await dbContext.Products
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == product.Id);
        Assert.True(deletedProduct!.IsDeleted);
    }
}