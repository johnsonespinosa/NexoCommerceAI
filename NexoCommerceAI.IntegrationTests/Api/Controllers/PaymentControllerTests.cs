using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexoCommerceAI.Application.Features.Auth.Models;
using NexoCommerceAI.Application.Features.Payments.Models;
using NexoCommerceAI.Infrastructure.Data;
using NexoCommerceAI.IntegrationTests.Helpers;
using Xunit;

namespace NexoCommerceAI.IntegrationTests.Api.Controllers;

public class PaymentControllerTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<string> GetAuthTokenAsync()
    {
        var loginRequest = new { Email = "customer@test.com", Password = "Customer123!" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var authResult = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return authResult!.Token;
    }

    [Fact]
    public async Task CreatePaymentIntent_ShouldReturnPaymentIntent_WhenOrderExists()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var order = await dbContext.Orders.FirstAsync();
        
        // Act
        var response = await _client.PostAsJsonAsync($"/api/payment/create-payment-intent/{order.Id}", (string?)null);
        var result = await response.Content.ReadFromJsonAsync<PaymentIntentResponse>();
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.NotNull(result.PaymentIntentId);
        Assert.NotNull(result.ClientSecret);
    }
}