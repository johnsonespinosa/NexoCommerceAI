using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Auth.Handlers;
using NexoCommerceAI.Application.Features.Auth.Queries;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Auth.Queries;

public class GetUserByEmailQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ILogger<GetUserByEmailQueryHandler>> _loggerMock;
    private readonly GetUserByEmailQueryHandler _handler;

    public GetUserByEmailQueryHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<GetUserByEmailQueryHandler>>();
        
        _handler = new GetUserByEmailQueryHandler(_userRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnUser_WhenEmailExists()
    {
        // Arrange
        var email = "test@mail.com";
        var query = new GetUserByEmailQuery(email);
        var role = Role.Create("Customer", "Regular customer");
        var user = User.Create("testuser", email, "hashed_password", role.Id);

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.UserName, result.UserName);
        Assert.Equal(email, result.Email);
        Assert.Equal(role.Name, result.Role);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenEmailDoesNotExist()
    {
        // Arrange
        var query = new GetUserByEmailQuery("nonexistent@mail.com");

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(query.Email, It.IsAny<CancellationToken>()))
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
        const string email = "deleted@mail.com";
        var query = new GetUserByEmailQuery(email);
        var role = Role.Create("Customer", "Regular customer");
        var user = User.Create("testuser", email, "hashed_password", role.Id);
        user.SoftDelete();

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null); // Repositorio no retorna usuarios eliminados

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }
}