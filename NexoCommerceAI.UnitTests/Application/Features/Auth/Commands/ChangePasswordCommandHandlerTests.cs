using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Auth.Commands;
using NexoCommerceAI.Application.Features.Auth.Handlers;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Auth.Commands;

public class ChangePasswordCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasherService> _passwordHasherMock;
    private readonly ChangePasswordCommandHandler _handler;

    public ChangePasswordCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasherService>();
        Mock<ILogger<ChangePasswordCommandHandler>> loggerMock = new();
        
        _handler = new ChangePasswordCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldChangePassword_WhenValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangePasswordCommand 
        { 
            UserId = userId, 
            CurrentPassword = "OldPassword123", 
            NewPassword = "NewPassword123",
            ConfirmNewPassword = "NewPassword123"
        };
        
        var role = Role.Create("Customer", "Regular customer");
        var user = User.Create("testuser", "test@mail.com", "old_hashed_password", role.Id);

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasherMock.Setup(p => p.Verify(command.CurrentPassword, user.PasswordHash)).Returns(true);
        _passwordHasherMock.Setup(p => p.Hash(command.NewPassword)).Returns("new_hashed_password");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal("new_hashed_password", user.PasswordHash);
        Assert.Null(user.RefreshToken);
        _userRepositoryMock.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenUserNotFound()
    {
        // Arrange
        var command = new ChangePasswordCommand 
        { 
            UserId = Guid.NewGuid(), 
            CurrentPassword = "OldPassword123", 
            NewPassword = "NewPassword123",
            ConfirmNewPassword = "NewPassword123"
        };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenCurrentPasswordIsIncorrect()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ChangePasswordCommand 
        { 
            UserId = userId, 
            CurrentPassword = "WrongPassword", 
            NewPassword = "NewPassword123",
            ConfirmNewPassword = "NewPassword123"
        };
        
        var role = Role.Create("Customer", "Regular customer");
        var user = User.Create("testuser", "test@mail.com", "hashed_password", role.Id);

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasherMock.Setup(p => p.Verify(command.CurrentPassword, user.PasswordHash)).Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => _handler.Handle(command, CancellationToken.None));
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}