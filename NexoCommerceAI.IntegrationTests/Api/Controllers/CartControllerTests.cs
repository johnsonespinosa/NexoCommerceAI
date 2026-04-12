using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexoCommerceAI.Application.Features.Auth.Models;
using NexoCommerceAI.Application.Features.Carts.Models;
using NexoCommerceAI.Infrastructure.Data;
using NexoCommerceAI.IntegrationTests.Helpers;
using Xunit;

namespace NexoCommerceAI.IntegrationTests.Api.Controllers;

public class CartControllerTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<string> GetAuthTokenAsync()
    {
        // Implementar obtención de token de autenticación
        var loginRequest = new { Email = "customer@test.com", Password = "Customer123!" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var authResult = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return authResult!.Token;
    }

    [Fact]
    public async Task GetCart_ShouldReturnCart_WhenAuthenticated()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/cart");
        var result = await response.Content.ReadFromJsonAsync<CartResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task AddToCart_ShouldAddItem_WhenValid()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var product = await dbContext.Products.FirstAsync();

        // Act
        var response = await _client.PostAsync($"/api/cart/items/{product.Id}?quantity=2", null);
        var result = await response.Content.ReadFromJsonAsync<CartResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Contains(result.Items, i => i.ProductId == product.Id);
    }
}