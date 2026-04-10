using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexoCommerceAI.Application.Features.Auth.Commands;
using NexoCommerceAI.Application.Features.Auth.Models;
using NexoCommerceAI.Application.Features.Auth.Queries;
using NexoCommerceAI.Application.Features.Users.Models;

namespace NexoCommerceAI.Api.Controllers.v1;

/// <summary>
/// Controller for authentication and user management.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
[ApiVersion("1.0")]
public class AuthController(IMediator mediator, ILogger<AuthController> logger) : ControllerBase
{
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    private readonly ILogger<AuthController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="request">The registration request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Authentication response with tokens.</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterCommand
        {
            UserName = request.UserName,
            Email = request.Email,
            Password = request.Password,
            ConfirmPassword = request.ConfirmPassword
        };
        
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Authenticates a user and returns tokens.
    /// </summary>
    /// <param name="request">The login request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Authentication response with tokens.</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand
        {
            Email = request.Email,
            Password = request.Password
        };
        
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Refreshes an expired JWT token using a refresh token.
    /// </summary>
    /// <param name="request">The refresh token request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>New authentication response with fresh tokens.</returns>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand
        {
            Token = request.Token,
            RefreshToken = request.RefreshToken
        };
        
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets the current authenticated user's profile.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's profile information.</returns>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();
        
        var query = new GetCurrentUserQuery(userId);
        var result = await _mediator.Send(query, cancellationToken);
        
        if (result == null)
            return NotFound();
        
        return Ok(result);
    }

    /// <summary>
    /// Updates the current user's profile.
    /// </summary>
    /// <param name="request">The update profile request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated user profile.</returns>
    [Authorize]
    [HttpPut("profile")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();
        
        var command = new UpdateUserProfileCommand
        {
            UserId = userId,
            UserName = request.UserName,
            Email = request.Email
        };
        
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Changes the current user's password.
    /// </summary>
    /// <param name="request">The change password request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content if successful.</returns>
    [Authorize]
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();
        
        var command = new ChangePasswordCommand
        {
            UserId = userId,
            CurrentPassword = request.CurrentPassword,
            NewPassword = request.NewPassword,
            ConfirmNewPassword = request.ConfirmNewPassword
        };
        
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
    
    /// <summary>
    /// Logs out the current user by revoking their refresh token.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content if successful.</returns>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> Logout(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();
    
        var command = new LogoutCommand { UserId = userId };
        await _mediator.Send(command, cancellationToken);
    
        return NoContent();
    }
    
    /// <summary>
    /// Gets a user by their unique identifier (Admin only).
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user if found; otherwise, a 404 status code.</returns>
    [Authorize(Roles = "Admin")]
    [HttpGet("users/{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UserResponse>> GetUserById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetUserByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
    
        if (result == null)
            return NotFound();
    
        return Ok(result);
    }

    /// <summary>
    /// Gets a user by their email address (Admin only).
    /// </summary>
    /// <param name="email">The email address of the user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user if found; otherwise, a 404 status code.</returns>
    [Authorize(Roles = "Admin")]
    [HttpGet("users/email/{email}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UserResponse>> GetUserByEmail(
        string email,
        CancellationToken cancellationToken)
    {
        var query = new GetUserByEmailQuery(email);
        var result = await _mediator.Send(query, cancellationToken);
    
        if (result == null)
            return NotFound();
    
        return Ok(result);
    }
}