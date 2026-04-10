using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Auth.Commands;
using NexoCommerceAI.Application.Features.Auth.Handlers;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Auth.Commands;

public class LogoutCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly LogoutCommandHandler _handler;

    public LogoutCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        Mock<ILogger<LogoutCommandHandler>> loggerMock = new();
        
        _handler = new LogoutCommandHandler(_userRepositoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldRevokeRefreshToken_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new LogoutCommand { UserId = userId };
        var role = Role.Create("Customer", "Regular customer");
        var user = User.Create("testuser", "test@mail.com", "hashed_password", role.Id);
        user.SetRefreshToken("valid_refresh_token", DateTime.UtcNow.AddDays(7));

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Null(user.RefreshToken);
        Assert.Null(user.RefreshTokenExpiryTime);
        _userRepositoryMock.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenUserNotFound()
    {
        // Arrange
        var command = new LogoutCommand { UserId = Guid.NewGuid() };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenUserHasNoRefreshToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new LogoutCommand { UserId = userId };
        var role = Role.Create("Customer", "Regular customer");
        var user = User.Create("testuser", "test@mail.com", "hashed_password", role.Id);

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Null(user.RefreshToken);
        _userRepositoryMock.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }
}