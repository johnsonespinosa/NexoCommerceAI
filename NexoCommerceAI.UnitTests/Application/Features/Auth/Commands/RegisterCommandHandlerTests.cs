using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Auth.Commands;
using NexoCommerceAI.Application.Features.Auth.Handlers;
using NexoCommerceAI.Domain.Entities;
using Xunit;
using Assert = Xunit.Assert;

namespace NexoCommerceAI.UnitTests.Application.Features.Auth.Commands;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IRoleRepository> _roleRepo = new();
    private readonly Mock<IPasswordHasherService> _hasher = new();
    private readonly Mock<IJwtTokenService> _jwt = new();
    private readonly Mock<ILogger<RegisterCommandHandler>> _logger = new();

    [Fact]
    public async Task Handle_ShouldRegisterUser_WhenValid()
    {
        // Arrange
        var command = new RegisterCommand 
        { 
            UserName = "testuser",
            Email = "test@mail.com",
            Password = "Password1",
            ConfirmPassword = "Password1"
        };
    
        var customerRole = Role.Create("Customer", "Regular customer with basic access");

        // Setup unique checks
        _userRepo.Setup(r => r.IsEmailUniqueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _userRepo.Setup(r => r.IsUserNameUniqueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    
        // Setup role repository
        _roleRepo.Setup(r => r.GetByNameAsync("Customer", It.IsAny<CancellationToken>()))
            .ReturnsAsync(customerRole);

        _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed_password");
        _jwt.Setup(j => j.GenerateRefreshToken()).Returns("refresh_token");
        _jwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("jwt_token");

        var handler = new RegisterCommandHandler(
            _userRepo.Object,
            _roleRepo.Object,
            _hasher.Object,
            _jwt.Object,
            _logger.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("testuser", result.UserName);
        Assert.Equal("test@mail.com", result.Email);
        Assert.Equal("jwt_token", result.Token);
        Assert.NotNull(result.RefreshToken);
    
        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _userRepo.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _hasher.Verify(h => h.Hash("Password1"), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowConflict_WhenEmailAlreadyExists()
    {
        // Arrange
        var command = new RegisterCommand 
        { 
            UserName = "testuser",
            Email = "existing@mail.com", 
            Password = "Password1",
            ConfirmPassword = "Password1"
        };

        _userRepo.Setup(r => r.IsEmailUniqueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new RegisterCommandHandler(
            _userRepo.Object,
            _roleRepo.Object,
            _hasher.Object,
            _jwt.Object,
            _logger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => 
            handler.Handle(command, CancellationToken.None));
    
        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _userRepo.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _hasher.Verify(h => h.Hash(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowConflict_WhenUsernameAlreadyExists()
    {
        // Arrange
        var command = new RegisterCommand 
        { 
            UserName = "existinguser",
            Email = "test@mail.com", 
            Password = "Password1",
            ConfirmPassword = "Password1"
        };

        _userRepo.Setup(r => r.IsEmailUniqueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _userRepo.Setup(r => r.IsUserNameUniqueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new RegisterCommandHandler(
            _userRepo.Object,
            _roleRepo.Object,
            _hasher.Object,
            _jwt.Object,
            _logger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => 
            handler.Handle(command, CancellationToken.None));
    
        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _hasher.Verify(h => h.Hash(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenCustomerRoleNotFound()
    {
        // Arrange
        var command = new RegisterCommand 
        { 
            UserName = "testuser",
            Email = "test@mail.com", 
            Password = "Password1",
            ConfirmPassword = "Password1"
        };

        _userRepo.Setup(r => r.IsEmailUniqueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _userRepo.Setup(r => r.IsUserNameUniqueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    
        _roleRepo.Setup(r => r.GetByNameAsync("Customer", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        var handler = new RegisterCommandHandler(
            _userRepo.Object,
            _roleRepo.Object,
            _hasher.Object,
            _jwt.Object,
            _logger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            handler.Handle(command, CancellationToken.None));
    
        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _hasher.Verify(h => h.Hash(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldUsePasswordHasherService_WhenCreatingUser()
    {
        // Arrange
        var command = new RegisterCommand 
        { 
            UserName = "testuser",
            Email = "test@mail.com", 
            Password = "MySecurePassword123",
            ConfirmPassword = "MySecurePassword123"
        };
    
        var customerRole = Role.Create("Customer", "Regular customer with basic access");
        var expectedHash = "hashed_password_123";

        _userRepo.Setup(r => r.IsEmailUniqueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _userRepo.Setup(r => r.IsUserNameUniqueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _roleRepo.Setup(r => r.GetByNameAsync("Customer", It.IsAny<CancellationToken>()))
            .ReturnsAsync(customerRole);
        _hasher.Setup(h => h.Hash("MySecurePassword123")).Returns(expectedHash);
        _jwt.Setup(j => j.GenerateRefreshToken()).Returns("refresh_token");
        _jwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("jwt_token");

        var handler = new RegisterCommandHandler(
            _userRepo.Object,
            _roleRepo.Object,
            _hasher.Object,
            _jwt.Object,
            _logger.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _hasher.Verify(h => h.Hash("MySecurePassword123"), Times.Once);
    }
}