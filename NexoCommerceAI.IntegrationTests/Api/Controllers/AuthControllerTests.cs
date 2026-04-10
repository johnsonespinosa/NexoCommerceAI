using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using NexoCommerceAI.Application.Features.Auth.Models;
using NexoCommerceAI.Application.Features.Users.Models;
using NexoCommerceAI.IntegrationTests.Helpers;
using Xunit;

namespace NexoCommerceAI.IntegrationTests.Api.Controllers;

public class AuthControllerTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory = factory;
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_ShouldCreateNewUser_WhenValid()
    {
        // Arrange
        var request = new RegisterRequest("newuser", "newuser@test.com", "Password123!", "Password123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(request.Email, result.Email);
        Assert.Equal(request.UserName, result.UserName);
        Assert.NotNull(result.Token);
    }

    [Fact]
    public async Task Login_ShouldReturnTokens_WhenCredentialsAreValid()
    {
        // Arrange
        var request = new LoginRequest("customer@test.com", "Customer123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.NotNull(result.Token);
        Assert.NotNull(result.RefreshToken);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
    {
        // Arrange
        var request = new LoginRequest("customer@test.com", "WrongPassword123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetCurrentUser_ShouldReturnUserProfile_WhenAuthenticated()
    {
        // Arrange
        var loginRequest = new LoginRequest("customer@test.com", "Customer123!");
        
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var authResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", authResult!.Token);

        // Act
        var response = await _client.GetAsync("/api/auth/me");
        var result = await response.Content.ReadFromJsonAsync<UserResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(authResult.Email, result.Email);
    }
}