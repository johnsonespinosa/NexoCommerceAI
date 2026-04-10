using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Roles.Commands;
using NexoCommerceAI.Application.Features.Roles.Handlers;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Roles.Commands;

public class ActivateRoleCommandHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly ActivateRoleCommandHandler _handler;

    public ActivateRoleCommandHandlerTests()
    {
        _roleRepositoryMock = new Mock<IRoleRepository>();
        Mock<ILogger<ActivateRoleCommandHandler>> loggerMock = new();
        
        _handler = new ActivateRoleCommandHandler(_roleRepositoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldActivateRole_WhenInactive()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var command = new ActivateRoleCommand(roleId);
        var role = Role.Create("Moderator", "Moderator role");
        role.Deactivate();

        _roleRepositoryMock.Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.True(role.IsActive);
        _roleRepositoryMock.Verify(r => r.UpdateAsync(role, It.IsAny<CancellationToken>()), Times.Once);
        _roleRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenAlreadyActive()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var command = new ActivateRoleCommand(roleId);
        var role = Role.Create("Moderator", "Moderator role");

        _roleRepositoryMock.Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
        _roleRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenRoleNotFound()
    {
        // Arrange
        var command = new ActivateRoleCommand(Guid.NewGuid());

        _roleRepositoryMock.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenRoleIsDeleted()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var command = new ActivateRoleCommand(roleId);
        var role = Role.Create("Moderator", "Moderator role");
        role.SoftDelete();

        _roleRepositoryMock.Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));
    }
}