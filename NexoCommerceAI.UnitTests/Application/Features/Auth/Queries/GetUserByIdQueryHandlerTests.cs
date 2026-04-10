using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Auth.Handlers;
using NexoCommerceAI.Application.Features.Auth.Queries;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Auth.Queries;

public class GetUserByIdQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ILogger<GetUserByIdQueryHandler>> _loggerMock;
    private readonly GetUserByIdQueryHandler _handler;

    public GetUserByIdQueryHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<GetUserByIdQueryHandler>>();
        
        _handler = new GetUserByIdQueryHandler(_userRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnUser_WhenExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);
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
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenUserNotFound()
    {
        // Arrange
        var query = new GetUserByIdQuery(Guid.NewGuid());

        _userRepositoryMock.Setup(r => r.GetByIdWithRoleAsync(query.Id, It.IsAny<CancellationToken>()))
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
        var query = new GetUserByIdQuery(userId);
        var role = Role.Create("Customer", "Regular customer");
        var user = User.Create("testuser", "test@mail.com", "hashed_password", role.Id);
        user.SoftDelete();

        _userRepositoryMock.Setup(r => r.GetByIdWithRoleAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null); // Repositorio no retorna usuarios eliminados

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }
}