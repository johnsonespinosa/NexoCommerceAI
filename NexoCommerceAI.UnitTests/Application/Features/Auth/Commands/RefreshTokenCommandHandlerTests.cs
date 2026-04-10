using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Auth.Commands;
using NexoCommerceAI.Application.Features.Auth.Handlers;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Auth.Commands;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IJwtTokenService> _jwtServiceMock;
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _jwtServiceMock = new Mock<IJwtTokenService>();
        Mock<ILogger<RefreshTokenCommandHandler>> loggerMock = new();
        
        _handler = new RefreshTokenCommandHandler(
            _userRepositoryMock.Object,
            _jwtServiceMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldRefreshToken_WhenValid()
    {
        // Arrange
        var command = new RefreshTokenCommand { Token = "expired_token", RefreshToken = "valid_refresh_token" };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Email, "test@mail.com")
        }));
        
        var role = Role.Create("Customer", "Regular customer");
        var user = User.Create("testuser", "test@mail.com", "hashed_password", role.Id);
        user.SetRefreshToken("valid_refresh_token", DateTime.UtcNow.AddDays(7));

        _jwtServiceMock.Setup(j => j.GetPrincipalFromExpiredToken(command.Token)).Returns(claimsPrincipal);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync("test@mail.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _jwtServiceMock.Setup(j => j.GenerateRefreshToken()).Returns("new_refresh_token");
        _jwtServiceMock.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("new_jwt_token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("new_jwt_token", result.Token);
        Assert.Equal("new_refresh_token", result.RefreshToken);
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenTokenIsInvalid()
    {
        // Arrange
        var command = new RefreshTokenCommand { Token = "invalid_token", RefreshToken = "refresh_token" };

        _jwtServiceMock.Setup(j => j.GetPrincipalFromExpiredToken(command.Token)).Returns((ClaimsPrincipal?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenUserNotFound()
    {
        // Arrange
        var command = new RefreshTokenCommand { Token = "expired_token", RefreshToken = "refresh_token" };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Email, "nonexistent@mail.com")
        }));

        _jwtServiceMock.Setup(j => j.GetPrincipalFromExpiredToken(command.Token)).Returns(claimsPrincipal);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync("nonexistent@mail.com", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenRefreshTokenIsInvalid()
    {
        // Arrange
        var command = new RefreshTokenCommand { Token = "expired_token", RefreshToken = "invalid_refresh_token" };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Email, "test@mail.com")
        }));
        
        var role = Role.Create("Customer", "Regular customer");
        var user = User.Create("testuser", "test@mail.com", "hashed_password", role.Id);
        user.SetRefreshToken("valid_refresh_token", DateTime.UtcNow.AddDays(7));

        _jwtServiceMock.Setup(j => j.GetPrincipalFromExpiredToken(command.Token)).Returns(claimsPrincipal);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync("test@mail.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenRefreshTokenIsExpired()
    {
        // Arrange
        var command = new RefreshTokenCommand { Token = "expired_token", RefreshToken = "expired_refresh_token" };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Email, "test@mail.com")
        }));
        
        var role = Role.Create("Customer", "Regular customer");
        var user = User.Create("testuser", "test@mail.com", "hashed_password", role.Id);
        user.SetRefreshToken("expired_refresh_token", DateTime.UtcNow.AddDays(-1)); // Expired

        _jwtServiceMock.Setup(j => j.GetPrincipalFromExpiredToken(command.Token)).Returns(claimsPrincipal);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync("test@mail.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => _handler.Handle(command, CancellationToken.None));
    }
}