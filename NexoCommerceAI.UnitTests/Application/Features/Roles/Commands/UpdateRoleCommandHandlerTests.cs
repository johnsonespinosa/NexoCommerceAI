using Microsoft.Extensions.Logging;
using Moq;
using NexoCommerceAI.Application.Common.Exceptions;
using NexoCommerceAI.Application.Common.Interfaces;
using NexoCommerceAI.Application.Features.Roles.Commands;
using NexoCommerceAI.Application.Features.Roles.Handlers;
using NexoCommerceAI.Domain.Entities;
using Xunit;

namespace NexoCommerceAI.UnitTests.Application.Features.Roles.Commands;

public class UpdateRoleCommandHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly UpdateRoleCommandHandler _handler;

    public UpdateRoleCommandHandlerTests()
    {
        _roleRepositoryMock = new Mock<IRoleRepository>();
        Mock<ILogger<UpdateRoleCommandHandler>> loggerMock = new();
        
        _handler = new UpdateRoleCommandHandler(_roleRepositoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateRole_WhenValid()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var command = new UpdateRoleCommand
        {
            Id = roleId,
            Name = "SuperAdmin",
            Description = "Super administrator with full access"
        };
        
        var role = Role.Create("Admin", "Administrator role");

        _roleRepositoryMock.Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);
        _roleRepositoryMock.Setup(r => r.IsRoleNameUniqueAsync(command.Name, roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("SuperAdmin", role.Name);
        Assert.Equal("Super administrator with full access", role.Description);
        _roleRepositoryMock.Verify(r => r.UpdateAsync(role, It.IsAny<CancellationToken>()), Times.Once);
        _roleRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenRoleNotFound()
    {
        // Arrange
        var command = new UpdateRoleCommand { Id = Guid.NewGuid(), Name = "NewName" };

        _roleRepositoryMock.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenUpdatingPredefinedRoleName()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var command = new UpdateRoleCommand
        {
            Id = roleId,
            Name = "NewAdminName",
            Description = "New description"
        };
        
        var role = Role.Create("Admin", "Administrator role");

        _roleRepositoryMock.Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));
        _roleRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}