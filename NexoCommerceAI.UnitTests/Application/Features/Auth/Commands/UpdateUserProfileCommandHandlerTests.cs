using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Auth.Commands;
using NexoCommerceAI.Application.Features.Auth.Handlers;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Auth.Commands;

public class UpdateUserProfileCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly UpdateUserProfileCommandHandler _handler;

    public UpdateUserProfileCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        Mock<ILogger<UpdateUserProfileCommandHandler>> loggerMock = new();
        
        _handler = new UpdateUserProfileCommandHandler(_userRepositoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateProfile_WhenValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserProfileCommand 
        { 
            UserId = userId, 
            UserName = "newusername", 
            Email = "newemail@mail.com" 
        };
        
        var role = Role.Create("Customer", "Regular customer");
        var user = User.Create("oldusername", "oldemail@mail.com", "hashed_password", role.Id);

        _userRepositoryMock.Setup(r => r.GetByIdWithRoleAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _userRepositoryMock.Setup(r => r.IsEmailUniqueAsync(command.Email, userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _userRepositoryMock.Setup(r => r.IsUserNameUniqueAsync(command.UserName, userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("newusername", user.UserName);
        Assert.Equal("newemail@mail.com", user.Email);
        Assert.NotNull(user.UpdatedAt);
        _userRepositoryMock.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenUserNotFound()
    {
        // Arrange
        var command = new UpdateUserProfileCommand 
        { 
            UserId = Guid.NewGuid(), 
            UserName = "newusername", 
            Email = "newemail@mail.com" 
        };

        _userRepositoryMock.Setup(r => r.GetByIdWithRoleAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowConflict_WhenEmailAlreadyExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserProfileCommand 
        { 
            UserId = userId, 
            UserName = "newusername", 
            Email = "existing@mail.com" 
        };
        
        var role = Role.Create("Customer", "Regular customer");
        var user = User.Create("oldusername", "oldemail@mail.com", "hashed_password", role.Id);

        _userRepositoryMock.Setup(r => r.GetByIdWithRoleAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _userRepositoryMock.Setup(r => r.IsEmailUniqueAsync(command.Email, userId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowConflict_WhenUsernameAlreadyExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserProfileCommand 
        { 
            UserId = userId, 
            UserName = "existinguser", 
            Email = "newemail@mail.com" 
        };
        
        var role = Role.Create("Customer", "Regular customer");
        var user = User.Create("oldusername", "oldemail@mail.com", "hashed_password", role.Id);

        _userRepositoryMock.Setup(r => r.GetByIdWithRoleAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _userRepositoryMock.Setup(r => r.IsEmailUniqueAsync(command.Email, userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _userRepositoryMock.Setup(r => r.IsUserNameUniqueAsync(command.UserName, userId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldNotCheckUniqueness_WhenEmailAndUsernameUnchanged()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new UpdateUserProfileCommand 
        { 
            UserId = userId, 
            UserName = "oldusername", 
            Email = "oldemail@mail.com" 
        };
        
        var role = Role.Create("Customer", "Regular customer");
        var user = User.Create("oldusername", "oldemail@mail.com", "hashed_password", role.Id);

        _userRepositoryMock.Setup(r => r.GetByIdWithRoleAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(r => r.IsEmailUniqueAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _userRepositoryMock.Verify(r => r.IsUserNameUniqueAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}