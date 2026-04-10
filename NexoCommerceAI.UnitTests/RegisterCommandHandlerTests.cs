using Ardalis.Specification;
using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Auth.Commands;
using NexoCommerceAI.Application.Features.Auth.Handlers;
using NexoCommerceAI.Domain.Entities;
using Xunit;
using Assert = Xunit.Assert;

namespace NexoCommerceAI.UnitTests;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IRepositoryAsync<Role>> _roleRepo = new();  // ✅ Added missing mock
    private readonly Mock<IPasswordHasherService> _hasher = new();
    private readonly Mock<IJwtTokenService> _jwt = new();
    private readonly Mock<ILogger<RegisterCommandHandler>> _logger = new();

    [Fact]
    public async Task Handle_ShouldRegisterUser_WhenValid()
    {
        // Arrange
        var command = new RegisterUserCommand("testuser", "test@mail.com", "Password1", "Password1");

        var customerRole = new Role 
        { 
            Id = Guid.NewGuid(),  // ✅ Use Guid instead of int
            Name = "Customer" 
        };

        _userRepo.Setup(r => r.IsEmailUniqueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _userRepo.Setup(r => r.IsUserNameUniqueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        // ✅ Setup the role repository mock
        _roleRepo.Setup(r => r.ListAsync(It.IsAny<ISpecification<Role>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Role> { customerRole });

        _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed_password");
        _jwt.Setup(j => j.GenerateRefreshToken()).Returns("refresh_token");
        _jwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("jwt_token");

        // ✅ Pass all 5 dependencies including roleRepository
        var handler = new RegisterCommandHandler(
            _userRepo.Object, 
            _roleRepo.Object,  // ✅ Added missing parameter
            _hasher.Object, 
            _jwt.Object, 
            _logger.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("testuser", result.UserName);
        Assert.Equal("jwt_token", result.Token);
        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowConflict_WhenEmailAlreadyExists()
    {
        // Arrange
        var command = new RegisterUserCommand("testuser", "existing@mail.com", "Password1", "Password1");

        _userRepo.Setup(r => r.IsEmailUniqueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);  // Email already exists

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
    }

    [Fact]
    public async Task Handle_ShouldThrowConflict_WhenUsernameAlreadyExists()
    {
        // Arrange
        var command = new RegisterUserCommand("existinguser", "test@mail.com", "Password1", "Password1");

        _userRepo.Setup(r => r.IsEmailUniqueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _userRepo.Setup(r => r.IsUserNameUniqueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);  // Username already exists

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
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenCustomerRoleNotFound()
    {
        // Arrange
        var command = new RegisterUserCommand("testuser", "test@mail.com", "Password1", "Password1");

        _userRepo.Setup(r => r.IsEmailUniqueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _userRepo.Setup(r => r.IsUserNameUniqueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        // Role not found - return empty list
        _roleRepo.Setup(r => r.ListAsync(It.IsAny<ISpecification<Role>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Role>());

        var handler = new RegisterCommandHandler(
            _userRepo.Object, 
            _roleRepo.Object, 
            _hasher.Object, 
            _jwt.Object, 
            _logger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => 
            handler.Handle(command, CancellationToken.None));
        
        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}