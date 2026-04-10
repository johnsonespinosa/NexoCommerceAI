using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Auth.Commands;
using NexoCommerceAI.Application.Features.Auth.Handlers;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Auth.Commands;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasherService> _passwordHasherMock;
    private readonly Mock<IJwtTokenService> _jwtServiceMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasherService>();
        _jwtServiceMock = new Mock<IJwtTokenService>();
        Mock<ILogger<LoginCommandHandler>> loggerMock = new();
        
        _handler = new LoginCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtServiceMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldLoginUser_WhenCredentialsAreValid()
    {
        // Arrange
        const string email = "test@mail.com";
        const string password = "Password123";
        var command = new LoginCommand { Email = email, Password = password };
        
        var role = Role.Create("Customer", "Regular customer");
        var user = User.Create("testuser", email, "hashed_password", role.Id);
        
        // Setup user repository
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        
        // Setup password verification
        _passwordHasherMock.Setup(p => p.Verify(password, user.PasswordHash))
            .Returns(true);
        
        // Setup JWT service
        _jwtServiceMock.Setup(j => j.GenerateRefreshToken()).Returns("new_refresh_token");
        _jwtServiceMock.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("jwt_token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(user.UserName, result.UserName);
        Assert.Equal("jwt_token", result.Token);
        Assert.Equal("new_refresh_token", result.RefreshToken);
        
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenUserNotFound()
    {
        // Arrange
        var command = new LoginCommand { Email = "nonexistent@mail.com", Password = "Password123" };

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => 
            _handler.Handle(command, CancellationToken.None));
        
        Assert.Equal("Invalid email or password", exception.Message);
        
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _jwtServiceMock.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenAccountIsInactive()
    {
        // Arrange
        const string email = "test@mail.com";
        var command = new LoginCommand { Email = email, Password = "Password123" };
        
        var role = Role.Create("Customer", "Regular customer");
        var user = User.Create("testuser", email, "hashed_password", role.Id);
        user.Deactivate(); // Desactivar cuenta

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => 
            _handler.Handle(command, CancellationToken.None));
        
        Assert.Equal("Account is inactive. Please contact support.", exception.Message);
        
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _jwtServiceMock.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenAccountIsDeleted()
    {
        // Arrange
        const string email = "test@mail.com";
        var command = new LoginCommand { Email = email, Password = "Password123" };
        
        var role = Role.Create("Customer", "Regular customer");
        var user = User.Create("testuser", email, "hashed_password", role.Id);
        user.SoftDelete(); // Marcar como eliminado

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => 
            _handler.Handle(command, CancellationToken.None));
        
        Assert.Equal("Account is inactive. Please contact support.", exception.Message);
        
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _jwtServiceMock.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenPasswordIsInvalid()
    {
        // Arrange
        const string email = "test@mail.com";
        const string password = "WrongPassword";
        var command = new LoginCommand { Email = email, Password = password };
        
        var role = Role.Create("Customer", "Regular customer");
        var user = User.Create("testuser", email, "hashed_password", role.Id);

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        
        // Setup password verification - returns false
        _passwordHasherMock.Setup(p => p.Verify(password, user.PasswordHash))
            .Returns(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => 
            _handler.Handle(command, CancellationToken.None));
        
        Assert.Equal("Invalid email or password", exception.Message);
        
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _jwtServiceMock.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldUpdateRefreshToken_OnSuccessfulLogin()
    {
        // Arrange
        const string email = "test@mail.com";
        const string password = "Password123";
        var command = new LoginCommand { Email = email, Password = password };
        
        var role = Role.Create("Customer", "Regular customer");
        var user = User.Create("testuser", email, "hashed_password", role.Id);
        var oldRefreshToken = user.RefreshToken;
        
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        
        _passwordHasherMock.Setup(p => p.Verify(password, user.PasswordHash))
            .Returns(true);
        
        _jwtServiceMock.Setup(j => j.GenerateRefreshToken()).Returns("new_refresh_token");
        _jwtServiceMock.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("jwt_token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(oldRefreshToken, user.RefreshToken);
        Assert.Equal("new_refresh_token", user.RefreshToken);
        Assert.NotNull(user.RefreshTokenExpiryTime);
        Assert.True(user.RefreshTokenExpiryTime > DateTime.UtcNow);
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectAuthResponse_OnSuccessfulLogin()
    {
        // Arrange
        const string email = "test@mail.com";
        const string password = "Password123";
        var command = new LoginCommand { Email = email, Password = password };
        
        var role = Role.Create("Customer", "Regular customer");
        var user = User.Create("testuser", email, "hashed_password", role.Id);
        const string expectedToken = "jwt_token_123";
        const string expectedRefreshToken = "refresh_token_456";
        
        _userRepositoryMock.Setup(repository => repository.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        
        _passwordHasherMock.Setup(passwordHasherService => passwordHasherService.Verify(password, user.PasswordHash))
            .Returns(true);
        
        _jwtServiceMock.Setup(jwtTokenService => jwtTokenService.GenerateRefreshToken()).Returns(expectedRefreshToken);
        _jwtServiceMock.Setup(jwtTokenService => jwtTokenService.GenerateToken(It.IsAny<User>())).Returns(expectedToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(user.UserName, result.UserName);
        Assert.Equal(expectedToken, result.Token);
        Assert.Equal(expectedRefreshToken, result.RefreshToken);
        Assert.Equal(role.Name, result.Role);
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
    }
}