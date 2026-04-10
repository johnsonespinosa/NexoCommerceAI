using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Roles.Commands;
using NexoCommerceAI.Application.Features.Roles.Handlers;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Roles.Commands;

public class DeleteRoleCommandHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly DeleteRoleCommandHandler _handler;

    public DeleteRoleCommandHandlerTests()
    {
        _roleRepositoryMock = new Mock<IRoleRepository>();
        Mock<ILogger<DeleteRoleCommandHandler>> loggerMock = new();
        
        _handler = new DeleteRoleCommandHandler(_roleRepositoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldSoftDeleteRole_WhenValid()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var command = new DeleteRoleCommand(roleId);
        var role = Role.Create("Moderator", "Moderator role");

        _roleRepositoryMock.Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);
        _roleRepositoryMock.Setup(r => r.HasUsersAssignedAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.True(role.IsDeleted);
        Assert.False(role.IsActive);
        _roleRepositoryMock.Verify(r => r.UpdateAsync(role, It.IsAny<CancellationToken>()), Times.Once);
        _roleRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenRoleNotFound()
    {
        // Arrange
        var command = new DeleteRoleCommand(Guid.NewGuid());

        _roleRepositoryMock.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenDeletingPredefinedRole()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var command = new DeleteRoleCommand(roleId);
        var role = Role.Create("Admin", "Administrator role");

        _roleRepositoryMock.Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenRoleHasUsersAssigned()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var command = new DeleteRoleCommand(roleId);
        var role = Role.Create("Moderator", "Moderator role");

        _roleRepositoryMock.Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);
        _roleRepositoryMock.Setup(r => r.HasUsersAssignedAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));
    }
}