using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexoCommerceAI.Application.Common.Models;
using NexoCommerceAI.Application.Features.Roles.Commands;
using NexoCommerceAI.Application.Features.Roles.Models;
using NexoCommerceAI.Application.Features.Roles.Queries;

namespace NexoCommerceAI.Api.Controllers.v1;

/// <summary>
/// Controller for managing roles in the system.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
[Consumes("application/json")]
[ApiVersion("1.0")]
public class RolesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<RolesController> _logger;

    public RolesController(IMediator mediator, ILogger<RolesController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Queries

    /// <summary>
    /// Gets a paginated list of roles.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<RoleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<RoleResponse>>> GetRoles(
        [FromQuery] PaginationParams pagination,
        CancellationToken cancellationToken)
    {
        var query = new GetRolesListQuery { Pagination = pagination };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets a role by its unique identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoleResponse>> GetRoleById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetRoleByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        
        if (result == null)
            return NotFound();
        
        return Ok(result);
    }

    /// <summary>
    /// Gets a role by its name.
    /// </summary>
    [HttpGet("name/{name}")]
    [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoleResponse>> GetRoleByName(
        string name,
        CancellationToken cancellationToken)
    {
        var query = new GetRoleByNameQuery(name);
        var result = await _mediator.Send(query, cancellationToken);
        
        if (result == null)
            return NotFound();
        
        return Ok(result);
    }

    #endregion

    #region Commands

    /// <summary>
    /// Creates a new role.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RoleResponse>> CreateRole(
        [FromBody] CreateRoleCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetRoleById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an existing role.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoleResponse>> UpdateRole(
        Guid id,
        [FromBody] UpdateRoleCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("ID in the URL does not match the ID in the request body.");
        
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Deletes (soft deletes) a role.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteRole(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteRoleCommand(id);
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result)
            return NotFound();
        
        return NoContent();
    }

    /// <summary>
    /// Activates a role.
    /// </summary>
    [HttpPatch("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ActivateRole(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new ActivateRoleCommand(id);
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result)
            return NotFound();
        
        return NoContent();
    }

    /// <summary>
    /// Deactivates a role.
    /// </summary>
    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeactivateRole(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeactivateRoleCommand(id);
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result)
            return NotFound();
        
        return NoContent();
    }

    #endregion
}