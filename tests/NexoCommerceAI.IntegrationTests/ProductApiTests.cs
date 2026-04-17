using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NexoCommerceAI.API.Contracts.Products;
using NexoCommerceAI.Application.Abstractions;
using NexoCommerceAI.Application.Common;
using NexoCommerceAI.Application.Products.CreateProduct;
using NexoCommerceAI.Domain.Entities;
using NexoCommerceAI.Infrastructure.Persistence;
using NexoCommerceAI.Infrastructure.Persistence.Repositories;
using System.Net;
using System.Net.Http.Json;

namespace NexoCommerceAI.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _databaseName = $"ProductApiTests_{Guid.NewGuid()}";
    private IServiceScope? _scope;
    private AppDbContext? _dbContext;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registrations
            var descriptorsToRemove = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                           d.ServiceType == typeof(AppDbContext))
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // Add InMemory database with unique name per test instance
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            // Register repositories (if they aren't already registered)
            services.TryAddScoped<IProductRepository, ProductRepository>();
            services.TryAddScoped<ICategoryRepository, CategoryRepository>();
        });
    }

    public async Task InitializeAsync()
    {
        _scope = Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        if (_dbContext != null)
        {
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.DisposeAsync();
        }

        _scope?.Dispose();
        await base.DisposeAsync();
    }

    public AppDbContext GetDbContext()
    {
        return _dbContext ?? throw new InvalidOperationException("Database context not initialized");
    }
}

public class ProductApiTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private HttpClient? _client;
    private AppDbContext? _dbContext;

    public ProductApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();
        _dbContext = _factory.GetDbContext();

        // Clean database before each test
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        await Task.CompletedTask;
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ReturnsCreated()
    {
        // Arrange
        var category = new Category("Test Category", "test-category");
        _dbContext!.Categories.Add(category);
        await _dbContext.SaveChangesAsync();

        var request = new CreateProductRequest(
            Name: "Test Product",
            CategoryId: category.Id,
            Description: "Test description",
            Price: 99.99m,
            CompareAtPrice: null,
            Sku: "TEST-001",
            Stock: 10,
            IsFeatured: false);

        // Act
        var response = await _client!.PostAsJsonAsync("/api/products", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CreateProductResult>();
        Assert.NotNull(result);
        Assert.Equal("Test Product", result.Name);
        Assert.Equal("TEST-001", result.Sku);
        Assert.Equal(category.Id, result.CategoryId);
        Assert.Equal(99.99m, result.Price);
        Assert.Equal(10, result.Stock);
    }

    [Fact]
    public async Task GetProducts_ReturnsEmptyList_WhenNoProductsExist()
    {
        // Act
        var response = await _client!.GetAsync("/api/products");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaginatedResult<object>>();
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task GetProductById_ReturnsNotFound_WhenProductDoesNotExist()
    {
        // Act
        var response = await _client!.GetAsync($"/api/products/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProduct_WithValidData_ReturnsOk()
    {
        // Arrange
        var category = new Category("Test Category", "test-category");
        _dbContext!.Categories.Add(category);
        await _dbContext.SaveChangesAsync();

        // Usar el factory method Create en lugar del constructor
        var product = Product.Create(
            name: "Test Product",
            categoryId: category.Id,
            description: "Description",
            price: 99.99m,
            sku: "TEST-001",
            stock: 10,
            isFeatured: false);

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        var updateRequest = new UpdateProductRequest(
            Name: "Updated Product",
            CategoryId: category.Id,
            Description: "Updated description",
            Price: 149.99m,
            CompareAtPrice: 199.99m,
            Sku: "TEST-001-UPDATED",
            Stock: 20,
            IsFeatured: true);

        // Act
        var response = await _client!.PutAsJsonAsync($"/api/products/{product.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Product>();
        Assert.NotNull(result);
        Assert.Equal("Updated Product", result.Name);
        Assert.Equal("TEST-001-UPDATED", result.Sku);
        Assert.Equal(149.99m, result.Price);
    }

    [Fact]
    public async Task DeleteProduct_ReturnsNoContent_WhenProductExists()
    {
        // Arrange
        var category = new Category("Test Category", "test-category");
        _dbContext!.Categories.Add(category);
        await _dbContext.SaveChangesAsync();

        var product = Product.Create(
            name: "Test Product",
            categoryId: category.Id,
            description: "Description",
            price: 99.99m,
            sku: "TEST-001",
            stock: 10,
            isFeatured: false);

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        // Act
        var response = await _client!.DeleteAsync($"/api/products/{product.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WithInvalidCategoryId_ReturnsValidationError()
    {
        // Arrange
        var request = new CreateProductRequest(
            Name: "Test Product",
            CategoryId: Guid.Empty,
            Description: "Test description",
            Price: 99.99m,
            CompareAtPrice: null,
            Sku: "TEST-001",
            Stock: 10,
            IsFeatured: false);

        // Act
        var response = await _client!.PostAsJsonAsync("/api/products", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("category", errorContent, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateProduct_WithDuplicateSku_ReturnsValidationError()
    {
        // Arrange
        var category = new Category("Test Category", "test-category");
        _dbContext!.Categories.Add(category);
        await _dbContext.SaveChangesAsync();

        // Crear primer producto usando el factory method
        var existingProduct = Product.Create(
            name: "Existing Product",
            categoryId: category.Id,
            description: "Description",
            price: 50.00m,
            sku: "DUPLICATE-SKU",
            stock: 5,
            isFeatured: false);

        _dbContext.Products.Add(existingProduct);
        await _dbContext.SaveChangesAsync();

        // Intentar crear otro producto con el mismo SKU
        var request = new CreateProductRequest(
            Name: "Duplicate Product",
            CategoryId: category.Id,
            Description: "Another description",
            Price: 75.00m,
            CompareAtPrice: null,
            Sku: "DUPLICATE-SKU",
            Stock: 3,
            IsFeatured: false);

        // Act
        var response = await _client!.PostAsJsonAsync("/api/products", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("SKU", errorContent, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateProduct_WithAutoGeneratedSlug_ShouldWork()
    {
        // Arrange
        var category = new Category("Test Category", "test-category");
        _dbContext!.Categories.Add(category);
        await _dbContext.SaveChangesAsync();

        var request = new CreateProductRequest(
            Name: "Test Product With Spaces",
            CategoryId: category.Id,
            Description: "Test description",
            Price: 99.99m,
            CompareAtPrice: null,
            Sku: null, // SKU auto-generado
            Stock: 10,
            IsFeatured: false);

        // Act
        var response = await _client!.PostAsJsonAsync("/api/products", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CreateProductResult>();
        Assert.NotNull(result);
        Assert.NotNull(result.Sku); // Verificar que se generó un SKU
        Assert.NotEmpty(result.Sku);
    }

    [Fact]
    public async Task UpdateProduct_WithInvalidPrice_ReturnsValidationError()
    {
        // Arrange
        var category = new Category("Test Category", "test-category");
        _dbContext!.Categories.Add(category);
        await _dbContext.SaveChangesAsync();

        var product = Product.Create(
            name: "Test Product",
            categoryId: category.Id,
            description: "Description",
            price: 99.99m,
            sku: "TEST-001",
            stock: 10,
            isFeatured: false);

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        var updateRequest = new UpdateProductRequest(
            Name: "Updated Product",
            CategoryId: category.Id,
            Description: "Updated description",
            Price: -10m, // Precio inválido
            CompareAtPrice: null,
            Sku: "TEST-001",
            Stock: 20,
            IsFeatured: true);

        // Act
        var response = await _client!.PutAsJsonAsync($"/api/products/{product.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("price", errorContent, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateProduct_WithoutRequiredFields_ReturnsValidationError()
    {
        // Arrange
        var request = new CreateProductRequest(
            Name: "", // Nombre vacío
            CategoryId: Guid.NewGuid(),
            Description: "Test description",
            Price: 99.99m,
            CompareAtPrice: null,
            Sku: "TEST-001",
            Stock: 10,
            IsFeatured: false);

        // Act
        var response = await _client!.PostAsJsonAsync("/api/products", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}