using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Auth.Handlers;
using NexoCommerceAI.Application.Features.Auth.Queries;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Auth.Queries;

public class GetCurrentUserQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly GetCurrentUserQueryHandler _handler;

    public GetCurrentUserQueryHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        Mock<ILogger<GetCurrentUserQueryHandler>> loggerMock = new();
        
        _handler = new GetCurrentUserQueryHandler(_userRepositoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetCurrentUserQuery(userId);
        var role = Role.Create("Customer", "Regular customer");
        var user = User.Create("testuser", "test@mail.com", "hashed_password", role.Id);

        _userRepositoryMock.Setup(r => r.GetByIdWithRoleAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.UserName, result.UserName);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(role.Name, result.Role);
        Assert.Equal(user.IsActive, result.IsActive);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenUserNotFound()
    {
        // Arrange
        var query = new GetCurrentUserQuery(Guid.NewGuid());

        _userRepositoryMock.Setup(r => r.GetByIdWithRoleAsync(query.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenUserIsDeleted()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetCurrentUserQuery(userId);
        var role = Role.Create("Customer", "Regular customer");
        var user = User.Create("testuser", "test@mail.com", "hashed_password", role.Id);
        user.SoftDelete();

        _userRepositoryMock.Setup(r => r.GetByIdWithRoleAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }
}