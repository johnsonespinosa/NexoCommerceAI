using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using NexoCommerceAI.Application.Features.Auth.Models;
using Xunit;
using Assert = Xunit.Assert;

namespace NexoCommerceAI.IntegrationTests.Api.Controllers;

public class AuthControllerIntegrationTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Register_Login_Refresh_Flow_ShouldWork()
    {
        var client = factory.CreateClient();

        // Register
        var registerRequest = new RegisterRequest("testuser", "test@mail.com", "Password1", "Password1");
        var response = await client.PostAsync("/api/v1/Auth/register",
            new StringContent(JsonSerializer.Serialize(registerRequest), Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();

        // Login
        var loginRequest = new LoginRequest("test@mail.com", "Password1");
        response = await client.PostAsync("/api/v1/Auth/login",
            new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();
        var loginResponse = JsonSerializer.Deserialize<AuthResponse>(await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(loginResponse);
        Assert.NotEmpty(loginResponse.Token);

        // Refresh
        var refreshRequest = new RefreshTokenRequest(loginResponse.RefreshToken);
        response = await client.PostAsync("/api/v1/Auth/refresh",
            new StringContent(JsonSerializer.Serialize(refreshRequest), Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();
        var refreshResponse = JsonSerializer.Deserialize<AuthResponse>(await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(refreshResponse);
        Assert.NotEqual(loginResponse.Token, refreshResponse.Token);
    }
}