using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Roles.Handlers;
using NexoCommerceAI.Application.Features.Roles.Queries;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Roles.Queries;

public class GetRoleByIdQueryHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly GetRoleByIdQueryHandler _handler;

    public GetRoleByIdQueryHandlerTests()
    {
        _roleRepositoryMock = new Mock<IRoleRepository>();
        Mock<ILogger<GetRoleByIdQueryHandler>> loggerMock = new();
        
        _handler = new GetRoleByIdQueryHandler(_roleRepositoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnRole_WhenExists()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var query = new GetRoleByIdQuery(roleId);
        var role = Role.Create("Admin", "Administrator role");

        _roleRepositoryMock.Setup(r => r.GetByIdWithUsersAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(role.Id, result.Id);
        Assert.Equal(role.Name, result.Name);
        Assert.Equal(role.Description, result.Description);
        Assert.Equal(role.IsActive, result.IsActive);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenRoleNotFound()
    {
        // Arrange
        var query = new GetRoleByIdQuery(Guid.NewGuid());

        _roleRepositoryMock.Setup(r => r.GetByIdWithUsersAsync(query.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }
}